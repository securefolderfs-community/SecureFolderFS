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
        public event EventHandler? AuthenticationCompleted;

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

        /// <summary>
        /// Call this from UI to confirm or reject an authentication request.
        /// </summary>
        public void ConfirmAuthentication(bool confirmed)
        {
            _authConfirmationTcs?.TrySetResult(confirmed);
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
            // Store previous client if any - we only track current for cleanup purposes
            var previousClient = Interlocked.Exchange(ref _currentClient, client);
            try
            {
                // Close any previous connection that might still be open
                previousClient?.Close();
            }
            catch { /* Ignore errors closing old connection */ }

            try
            {
                await using var stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    var message = await ReceiveMessageAsync(stream, cancellationToken);
                    var messageType = (MessageType)message[0];

                    switch (messageType)
                    {
                        case MessageType.PairingRequest:
                            await HandlePairingRequestAsync(stream, message, cancellationToken);
                            CleanupSessionState();
                            break;

                        case MessageType.SecureSessionRequest:
                            await HandleSecureSessionRequestAsync(stream, message, cancellationToken);
                            break;

                        case MessageType.SecureAuthRequest:
                            await HandleSecureAuthRequestAsync(stream, message, cancellationToken);
                            CleanupSessionState();
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
                // Only clean up if this is still the current client
                Interlocked.CompareExchange(ref _currentClient, null, client);

                // Clean up the connection
                try
                {
                    client.Close();
                    client.Dispose();
                }
                catch { /* Ignore disposal errors */ }

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
            _pendingDesktopName = reader.ReadString(); // Read desktop name
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
            PairingRequested?.Invoke(this, new PairingRequestViewModel(_pendingDesktopName, string.Empty, verificationCode));
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
            ParsePairingConfirm(confirmMessage, out var credentialId, out var vaultName, out var pairingId, out var challenge);

            _pendingCredentialId = credentialId;
            _pendingVaultName = vaultName;
            _pendingPairingId = pairingId;

            // Create and enroll credential with persistent challenge
            var credential = await CreateEnrolledCredentialAsync(credentialId, vaultName, pairingId, challenge);

            // Get the encryption key to decrypt HMAC key for computing the initial HMAC
            var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(pairingId);
            if (encryptionKey == null)
            {
                await SendMessageAsync(stream, [(byte)MessageType.PairingRejected], cancellationToken);
                CleanupSession();
                return;
            }

            try
            {
                // Decrypt HMAC key so we can compute HMAC
                credential.DecryptHmacKey(encryptionKey);

                // Compute HMAC over the persistent challenge data
                // HMAC is deterministic: same key + same data = same result every time
                var challengeData = BuildChallengeData(credentialId, challenge);
                var hmacResult = credential.ComputeHmac(challengeData);

                // Send pairing complete with HMAC result (this replaces the signature)
                var completeMessage = CreatePairingComplete(hmacResult);
                await SendMessageAsync(stream, completeMessage, cancellationToken);

                // Clear the decrypted key from memory
                credential.ClearDecryptedKey();

                // Notify UI that enrollment is complete
                EnrollmentCompleted?.Invoke(this, credential);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(encryptionKey);
            }
        }

        private async Task<CredentialViewModel> CreateEnrolledCredentialAsync(
            string credentialId, string vaultName, string pairingId, byte[] challenge)
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
                challenge,
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
            var keyLength = reader.ReadInt32();
            var desktopEcdhPublicKey = reader.ReadBytes(keyLength);;

            // Find credential by pairing ID
            var credential = _credentialStoreModel.GetByPairingId(pairingId);
            if (credential == null)
            {
                await SendAuthenticationRejectedAsync(stream, "Unknown pairing", cancellationToken);
                return;
            }

            // Store credential for later use in auth request
            _currentCredential = credential;

            // Generate fresh ECDH keypair for this session (ephemeral)
            _ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myPublicKey = _ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive session secret using ECDH (transport security only)
            _sharedSecret = SecureChannelModel.DeriveSharedSecret(_ecdhKeyPair, desktopEcdhPublicKey);

            // Generate our session nonce
            var mobileNonce = new byte[16];
            RandomNumberGenerator.Fill(mobileNonce);

            // Derive session key from ECDH secret + both nonces
            var combinedNonce = new byte[desktopNonce.Length + mobileNonce.Length];
            desktopNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, desktopNonce.Length);

            _secureChannel = new SecureChannelModel(_sharedSecret, combinedNonce);

            // Send response with our ECDH public key
            var response = CreateSecureSessionAccepted(mobileNonce, myPublicKey);
            await SendMessageAsync(stream, response, cancellationToken);
        }

        private CredentialViewModel? _currentCredential;
        private TaskCompletionSource<bool>? _authConfirmationTcs;

        private async Task HandleSecureAuthRequestAsync(NetworkStream stream, byte[] message,
            CancellationToken cancellationToken)
        {
            if (_secureChannel == null || _currentCredential == null)
            {
                await SendAuthenticationRejectedAsync(stream, "No session", cancellationToken);
                return;
            }

            try
            {
                // Decrypt request
                var encryptedPayload = message.AsSpan(1).ToArray();
                var decryptedPayload = _secureChannel.Decrypt(encryptedPayload);

                // Parse request (persistent challenge from desktop)
                using var ms = new MemoryStream(decryptedPayload);
                using var reader = new BinaryReader(ms);

                var credentialId = reader.ReadString();
                var challengeLength = reader.ReadInt32();
                var persistentChallenge = reader.ReadBytes(challengeLength);
                var timestamp = reader.ReadInt64();

                // Validate timestamp (for replay protection)
                var requestTime = DateTimeOffset.FromUnixTimeSeconds(timestamp);
                var age = DateTimeOffset.UtcNow - requestTime;

                if (Math.Abs(age.TotalSeconds) > CHALLENGE_VALIDITY_SECONDS)
                {
                    await SendAuthenticationRejectedAsync(stream, "Request expired", cancellationToken);
                    return;
                }

                // Verify credential matches
                if (_currentCredential.CredentialId != credentialId)
                {
                    await SendAuthenticationRejectedAsync(stream, "Credential mismatch", cancellationToken);
                    return;
                }

                // Verify the persistent challenge matches what we have stored
                var storedChallenge = _currentCredential.Challenge;
                if (storedChallenge == null || !persistentChallenge.SequenceEqual(storedChallenge))
                {
                    await SendAuthenticationRejectedAsync(stream, "Challenge mismatch", cancellationToken);
                    return;
                }

                // Create a TaskCompletionSource for user confirmation
                _authConfirmationTcs = new TaskCompletionSource<bool>();
                var authInfo = new AuthenticationRequestModel(_currentCredential.VaultName, _currentCredential.MachineName, _currentCredential.DisplayName);

                // Notify UI about auth request (could require biometric)
                AuthenticationRequested?.Invoke(this, authInfo);

                // Wait for user to accept or reject
                var userConfirmed = await _authConfirmationTcs.Task;
                if (!userConfirmed)
                {
                    await SendAuthenticationRejectedAsync(stream, "User rejected", cancellationToken);
                    return;
                }

                // Decrypt HMAC key using stored encryption key
                var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(_currentCredential.PairingId);
                if (encryptionKey == null)
                {
                    await SendAuthenticationRejectedAsync(stream, "Key error", cancellationToken);
                    return;
                }

                try
                {
                    _currentCredential.DecryptHmacKey(encryptionKey);

                    // Compute HMAC over the challenge data
                    // HMAC is deterministic: same key + same data = same result every time
                    var challengeData = BuildChallengeData(credentialId, persistentChallenge);
                    var hmacResult = _currentCredential.ComputeHmac(challengeData);

                    // Encrypt and send response
                    var encryptedHmac = _secureChannel.Encrypt(hmacResult);
                    var response = new byte[1 + encryptedHmac.Length];
                    response[0] = (byte)MessageType.SecureAuthResponse;
                    encryptedHmac.CopyTo(response, 1);

                    await SendMessageAsync(stream, response, cancellationToken);

                    // Notify UI that authentication completed successfully
                    AuthenticationCompleted?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    // Clear decrypted key from memory
                    _currentCredential.ClearDecryptedKey();
                    CryptographicOperations.ZeroMemory(encryptionKey);
                }
            }
            catch (CryptographicException ex)
            {
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

        /// <summary>
        /// Builds the data for HMAC computation.
        /// Must match desktop's BuildChallengeData exactly.
        /// HMAC is deterministic: same key + same data = same result.
        /// </summary>
        private static byte[] BuildChallengeData(string credentialId, byte[] persistentChallenge)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // HMAC input: CID + persistent challenge
            writer.Write(Encoding.UTF8.GetBytes(credentialId));
            writer.Write(persistentChallenge);

            return ms.ToArray();
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

        private static void ParsePairingConfirm(byte[] message, out string credentialId, out string vaultName, out string pairingId, out byte[] challenge)
        {
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            credentialId = reader.ReadString();
            vaultName = reader.ReadString();
            pairingId = reader.ReadString();
            var challengeLength = reader.ReadInt32();
            challenge = reader.ReadBytes(challengeLength);
        }

        private static byte[] BuildPersistentChallengeData(string credentialId, byte[] challenge)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            // Sign: CID + persistent challenge
            // This is deterministic - same input always produces same signature
            writer.Write(Encoding.UTF8.GetBytes(credentialId));
            writer.Write(challenge);

            return ms.ToArray();
        }

        /// <summary>
        /// Creates pairing complete message with HMAC result.
        /// </summary>
        private static byte[] CreatePairingComplete(byte[] hmacResult)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.PairingComplete);
            writer.Write(hmacResult.Length);
            writer.Write(hmacResult);

            return ms.ToArray();
        }

        private static byte[] CreateSecureSessionAccepted(byte[] nonce, byte[] ecdhPublicKey)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write((byte)MessageType.SecureSessionAccepted);
            writer.Write(nonce.Length);
            writer.Write(nonce);
            writer.Write(ecdhPublicKey.Length);
            writer.Write(ecdhPublicKey);

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
            CleanupSessionState();

            _pendingCredentialId = null;
            _pendingVaultName = null;
            _pendingPairingId = null;
            _pendingDesktopName = null;
        }

        /// <summary>
        /// Cleans up cryptographic session state without clearing pending operation state.
        /// Call this after a successful operation to prepare for the next one.
        /// </summary>
        private void CleanupSessionState()
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

            _pairingConfirmationTcs = null;
            _authConfirmationTcs = null;
            _currentCredential = null;
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
            AuthenticationCompleted = null;
        }
    }
}
