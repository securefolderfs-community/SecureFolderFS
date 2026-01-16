using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.PhoneLink.Enums;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.PhoneLink.ViewModels;
using static SecureFolderFS.Sdk.PhoneLink.Constants;

namespace SecureFolderFS.Sdk.PhoneLink.Services
{
    /// <inheritdoc cref="IDeviceLinkService"/>
    public sealed class DeviceLinkService : IDeviceLinkService
    {
        private readonly string _deviceId;
        private readonly string _deviceName;
        private readonly CredentialsStoreModel _credentialStoreModel;
        private UdpClient? _discoveryListener;
        private TcpClient? _currentClient;
        private TcpListener? _tcpListener;
        private CancellationTokenSource? _listenerCts;
        private TaskCompletionSource<bool>? _pairingConfirmationTcs;
        private string? _pendingCredentialId;
        private string? _pendingVaultName;
        private string? _pendingPairingId;
        private string? _pendingDesktopName;
        private ECDiffieHellman? _ecdhKeyPair;
        private byte[]? _sharedSecret;
        private SecureChannelModel? _secureChannel;
        private bool _disposed;

        public DeviceLinkService(string deviceName, string deviceId, CredentialsStoreModel credentialStoreModel)
        {
            _deviceName = deviceName;
            _deviceId = deviceId;
            _credentialStoreModel = credentialStoreModel;
        }

        /// <inheritdoc/>
        public event EventHandler<CredentialViewModel>? EnrollmentCompleted;

        /// <inheritdoc/>
        public event EventHandler<PairingRequestViewModel>? PairingRequested;

        /// <inheritdoc/>
        public event EventHandler<AuthenticationRequestModel>? AuthenticationRequested;

        /// <inheritdoc/>
        public event EventHandler<string>? VerificationCodeReady;

        /// <inheritdoc/>
        public event EventHandler? Disconnected;

        /// <inheritdoc/>
        public bool IsListening { get; private set; }

        /// <inheritdoc/>
        public Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            if (IsListening)
                return Task.CompletedTask;

            _listenerCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            // Start UDP discovery listener
            _discoveryListener = new UdpClient(DISCOVERY_PORT);
            _ = Task.Run(() => ListenForDiscoveryAsync(_listenerCts.Token));

            // Start TCP listener
            _tcpListener = new TcpListener(IPAddress.Any, COMMUNICATION_PORT);
            _tcpListener.Start();
            _ = Task.Run(() => AcceptConnectionsAsync(_listenerCts.Token));

            IsListening = true;
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void StopListening()
        {
            _listenerCts?.Cancel();
            _discoveryListener?.Close();
            _tcpListener?.Stop();
            _currentClient?.Close();

            IsListening = false;
        }

        /// <inheritdoc/>
        public void ConfirmPairingRequest(bool value)
        {
            _pairingConfirmationTcs?.TrySetResult(value);
        }

        private async Task ListenForDiscoveryAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _discoveryListener!.ReceiveAsync(cancellationToken);
                    if (!ValidateDiscoveryRequest(result.Buffer))
                        continue;

                    var response = CreateDiscoveryResponse();
                    await _discoveryListener.SendAsync(response, response.Length, result.RemoteEndPoint);
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        private static bool ValidateDiscoveryRequest(byte[] buffer)
        {
            if (buffer.Length < DISCOVERY_MAGIC.Length + 2)
                return false;

            var magic = buffer.AsSpan(0, DISCOVERY_MAGIC.Length).ToArray();
            return magic.SequenceEqual(DISCOVERY_MAGIC);
        }

        private byte[] CreateDiscoveryResponse()
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(DISCOVERY_MAGIC);
            writer.Write(PROTOCOL_VERSION);
            writer.Write((byte)MessageType.DiscoveryResponse);
            writer.Write(_deviceId);
            writer.Write(_deviceName);
            writer.Write(COMMUNICATION_PORT);

            // Include empty public key placeholder (actual signing key comes after pairing)
            writer.Write(0);

            return ms.ToArray();
        }

        private async Task AcceptConnectionsAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var client = await _tcpListener!.AcceptTcpClientAsync(cancellationToken);
                    _ = Task.Run(() => HandleConnectionAsync(client, cancellationToken));
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        private async Task HandleConnectionAsync(TcpClient client, CancellationToken cancellationToken)
        {
            _currentClient = client;

            try
            {
                await using var stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await ReceiveMessageAsync(stream, cancellationToken);
                    var messageType = (MessageType)message[0];

                    switch (messageType)
                    {
                        case MessageType.PairingRequest:
                            await HandlePairingRequestAsync(stream, message, cancellationToken);
                            break;

                        case MessageType.SecureSessionRequest:
                            await HandleSecureSessionRequestAsync(stream, message, cancellationToken);
                            break;

                        case MessageType.SecureAuthRequest:
                            await HandleSecureAuthRequestAsync(stream, message, cancellationToken);
                            break;

                        default:
                            Debugger.Break();
                            break;
                    }
                }
            }
            catch (IOException)
            {
                // Connection closed
            }
            catch (Exception ex)
            {
                _ = ex;
            }
            finally
            {
                _currentClient = null;
                CleanupSession();
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Secure Pairing

        private async Task HandlePairingRequestAsync(NetworkStream stream, byte[] message,
            CancellationToken cancellationToken)
        {
            // Parse desktop's ECDH public key
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var keyLength = reader.ReadInt32();
            var desktopEcdhPublicKey = reader.ReadBytes(keyLength);

            // Generate our ECDH keypair
            _ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myPublicKey = _ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive shared secret BEFORE sending response (so we can show code immediately)
            _sharedSecret = SecureChannelModel.DeriveSharedSecret(_ecdhKeyPair, desktopEcdhPublicKey);

            // Compute verification code
            var verificationCode = SecureChannelModel.ComputeVerificationCode(_sharedSecret);

            // Send our public key response
            var response = CreatePairingResponse(myPublicKey);
            await SendMessageAsync(stream, response, cancellationToken);

            // Notify UI to display verification code and wait for user confirmation
            // This happens BEFORE we receive PairingConfirm from desktop
            _pairingConfirmationTcs = new TaskCompletionSource<bool>();

            // Show the pairing request with verification code
            PairingRequested?.Invoke(this, new PairingRequestViewModel("(awaiting desktop...)", string.Empty, verificationCode));
            VerificationCodeReady?.Invoke(this, verificationCode);

            // Wait for user confirmation on mobile (user confirms code matches)
            var userConfirmed = await _pairingConfirmationTcs.Task;
            if (!userConfirmed)
            {
                // We need to wait for desktop's message and reject it
                try
                {
                    _ = await ReceiveMessageAsync(stream, cancellationToken);
                    await SendMessageAsync(stream, [(byte)MessageType.PairingRejected], cancellationToken);
                }
                catch (Exception)
                {
                    // Connection may have closed
                }

                CleanupSession();
                return;
            }

            // Now wait for pairing confirmation from desktop (which includes vault info)
            var confirmMessage = await ReceiveMessageAsync(stream, cancellationToken);
            var confirmType = (MessageType)confirmMessage[0];

            if (confirmType == MessageType.PairingRejected)
            {
                CleanupSession();
                return;
            }

            if (confirmType != MessageType.PairingConfirm)
            {
                CleanupSession();
                return;
            }

            // Parse pairing confirmation
            ParsePairingConfirm(confirmMessage, out var credentialId, out var vaultName, out var pairingId);

            _pendingCredentialId = credentialId;
            _pendingVaultName = vaultName;
            _pendingPairingId = pairingId;
            _pendingDesktopName = "Desktop"; // Could be parsed from connection request

            try
            {
                // Create and enroll credential (user already confirmed above)
                var credential = await CreateEnrolledCredentialAsync(credentialId, vaultName, pairingId);

                // Send pairing complete with signing public key
                var completeMessage = CreatePairingComplete(credential.PublicSigningKey);
                await SendMessageAsync(stream, completeMessage, cancellationToken);

                // Notify UI that enrollment is complete
                EnrollmentCompleted?.Invoke(this, credential);
            }
            catch (Exception ex)
            {
                _ = ex;
                await SendMessageAsync(stream, [(byte)MessageType.PairingRejected], cancellationToken);
                CleanupSession();
            }
        }

        private async Task<CredentialViewModel> CreateEnrolledCredentialAsync(
            string credentialId, string vaultName, string pairingId)
        {
            var credential = new CredentialViewModel()
            {
                DisplayName = vaultName,
                CreatedAt = DateTime.UtcNow
            };

            await _credentialStoreModel.EnrollCredentialAsync(
                credential,
                credentialId,
                vaultName,
                _pendingDesktopName ?? "Desktop",
                pairingId,
                _sharedSecret!);

            // Return the credential directly - it's been enrolled and added to the store
            return credential;
        }

        #endregion

        #region Secure Authentication

        private async Task HandleSecureSessionRequestAsync(NetworkStream stream, byte[] message,
            CancellationToken cancellationToken)
        {
            // Parse request
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var pairingId = reader.ReadString();
            var nonceLength = reader.ReadInt32();
            var desktopNonce = reader.ReadBytes(nonceLength);

            // Find credential by pairing ID
            var credential = _credentialStoreModel.GetByPairingId(pairingId);
            if (credential == null)
            {
                await SendAuthenticationRejectedAsync(stream, "Unknown pairing", cancellationToken);
                return;
            }

            // Load shared secret
            _sharedSecret = await _credentialStoreModel.GetSharedSecretAsync(pairingId);
            if (_sharedSecret == null)
            {
                await SendAuthenticationRejectedAsync(stream, "Session error", cancellationToken);
                return;
            }

            // Generate our session nonce
            var mobileNonce = new byte[16];
            RandomNumberGenerator.Fill(mobileNonce);

            // Derive session key
            var combinedNonce = new byte[desktopNonce.Length + mobileNonce.Length];
            desktopNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, desktopNonce.Length);

            _secureChannel = new SecureChannelModel(_sharedSecret, combinedNonce);

            // Send response
            var response = CreateSecureSessionAccepted(mobileNonce);
            await SendMessageAsync(stream, response, cancellationToken);
        }

        private async Task HandleSecureAuthRequestAsync(NetworkStream stream, byte[] message,
            CancellationToken cancellationToken)
        {
            if (_secureChannel == null || _sharedSecret == null)
            {
                await SendAuthenticationRejectedAsync(stream, "No session", cancellationToken);
                return;
            }

            try
            {
                // Decrypt request
                var encryptedPayload = message.AsSpan(1).ToArray();
                var decryptedPayload = _secureChannel.Decrypt(encryptedPayload);

                // Parse request
                using var ms = new MemoryStream(decryptedPayload);
                using var reader = new BinaryReader(ms);

                var credentialId = reader.ReadString();
                var challengeLength = reader.ReadInt32();
                var challenge = reader.ReadBytes(challengeLength);
                var timestamp = reader.ReadInt64();
                var nonceLength = reader.ReadInt32();
                var nonce = reader.ReadBytes(nonceLength);

                // Validate timestamp
                var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                var age = DateTimeOffset.UtcNow - requestTime;

                if (Math.Abs(age.TotalSeconds) > CHALLENGE_VALIDITY_SECONDS)
                {
                    await SendAuthenticationRejectedAsync(stream, "Challenge expired", cancellationToken);
                    return;
                }

                // Find credential
                var credential = _credentialStoreModel.GetByCredentialId(credentialId);
                if (credential == null)
                {
                    await SendAuthenticationRejectedAsync(stream, "Credential not found", cancellationToken);
                    return;
                }

                // Notify UI about auth request (could require biometric)
                var authInfo = new AuthenticationRequestModel(credential.VaultName, credential.DisplayName);
                AuthenticationRequested?.Invoke(this, authInfo);

                // Decrypt signing key and sign
                credential.DecryptSigningKey(_sharedSecret);

                try
                {
                    // Build data to sign: CID + challenge + timestamp + nonce
                    var signedData = BuildSignedData(credentialId, challenge, timestamp, nonce);
                    var signature = credential.SignData(signedData);

                    // Encrypt and send response
                    var encryptedSignature = _secureChannel.Encrypt(signature);
                    var response = new byte[KeyTraits.MESSAGE_BYTE_LENGTH + encryptedSignature.Length];
                    response[0] = (byte)MessageType.SecureAuthResponse;
                    encryptedSignature.CopyTo(response, KeyTraits.MESSAGE_BYTE_LENGTH);

                    await SendMessageAsync(stream, response, cancellationToken);
                }
                finally
                {
                    // Clear decrypted key from memory
                    credential.ClearDecryptedKey();
                }
            }
            catch (CryptographicException ex)
            {
                _ = ex;
                await SendAuthenticationRejectedAsync(stream, "Decryption failed", cancellationToken);
            }
        }

        private async Task SendAuthenticationRejectedAsync(NetworkStream stream, string reason,
            CancellationToken cancellationToken)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.AuthenticationRejected);
            writer.Write(reason);

            await SendMessageAsync(stream, ms.ToArray(), cancellationToken);
        }

        private static byte[] BuildSignedData(string credentialId, byte[] challenge, long timestamp, byte[] nonce)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(Encoding.UTF8.GetBytes(credentialId));
            writer.Write(challenge);
            writer.Write(timestamp);
            writer.Write(nonce);

            return ms.ToArray();
        }

        #endregion

        #region Message Helpers

        private static byte[] CreatePairingResponse(byte[] ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingResponse);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

            return ms.ToArray();
        }

        private static void ParsePairingConfirm(byte[] message, out string credentialId, out string vaultName, out string pairingId)
        {
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            credentialId = reader.ReadString();
            vaultName = reader.ReadString();
            pairingId = reader.ReadString();
        }

        private static byte[] CreatePairingComplete(byte[] signingPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingComplete);
            writer.Write(signingPublicKey.Length);
            writer.Write(signingPublicKey);

            return ms.ToArray();
        }

        private static byte[] CreateSecureSessionAccepted(byte[] nonce)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SecureSessionAccepted);
            writer.Write(nonce.Length);
            writer.Write(nonce);

            return ms.ToArray();
        }

        private static async Task SendMessageAsync(NetworkStream stream, byte[] message, CancellationToken cancellationToken)
        {
            var lengthBytes = BitConverter.GetBytes(message.Length);
            await stream.WriteAsync(lengthBytes, cancellationToken);
            await stream.WriteAsync(message, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }

        private static async Task<byte[]> ReceiveMessageAsync(NetworkStream stream, CancellationToken cancellationToken)
        {
            var lengthBytes = new byte[4];
            var bytesRead = await stream.ReadAsync(lengthBytes, cancellationToken);
            if (bytesRead < 4)
                throw new IOException("Connection closed");

            var length = BitConverter.ToInt32(lengthBytes);
            var message = new byte[length];
            var totalRead = 0;

            while (totalRead < length)
            {
                bytesRead = await stream.ReadAsync(message.AsMemory(totalRead, length - totalRead), cancellationToken);
                if (bytesRead == 0)
                    throw new IOException("Connection closed");
                totalRead += bytesRead;
            }

            return message;
        }

        private void CleanupSession()
        {
            _secureChannel?.Dispose();
            _secureChannel = null;

            _ecdhKeyPair?.Dispose();
            _ecdhKeyPair = null;

            if (_sharedSecret != null)
            {
                CryptographicOperations.ZeroMemory(_sharedSecret);
                _sharedSecret = null;
            }

            _pendingCredentialId = null;
            _pendingVaultName = null;
            _pendingPairingId = null;
            _pairingConfirmationTcs = null;
        }

        #endregion

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            StopListening();
            CleanupSession();
            _discoveryListener?.Dispose();

            Disconnected = null;
            PairingRequested = null;
            EnrollmentCompleted = null;
            VerificationCodeReady = null;
            AuthenticationRequested = null;
        }
    }
}
