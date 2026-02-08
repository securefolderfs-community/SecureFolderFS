using System;
using System.Diagnostics;
using System.IO;
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
        private readonly CredentialsStoreModel _credentialStoreModel;
        private readonly DeviceConnectionListener _connectionListener;
        private CancellationTokenSource? _serviceCts;
        private TaskCompletionSource<bool>? _pairingConfirmationTcs;
        private TaskCompletionSource<bool>? _authConfirmationTcs;
        private CredentialViewModel? _currentCredential;
        private ECDiffieHellman? _ecdhKeyPair;
        private byte[]? _sharedSecret;
        private SecureChannelModel? _secureChannel;
        private bool _disposed;

        public DeviceLinkService(string deviceName, string deviceId, CredentialsStoreModel credentialStoreModel)
        {
            _credentialStoreModel = credentialStoreModel;
            _connectionListener = new DeviceConnectionListener(deviceId, deviceName);
            _connectionListener.ConnectionAccepted += OnConnectionAccepted;
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
        public bool IsListening => _connectionListener.IsListening;

        /// <inheritdoc/>
        public Task StartListeningAsync(CancellationToken cancellationToken = default)
        {
            _serviceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            return _connectionListener.StartListeningAsync(_serviceCts.Token);
        }

        /// <inheritdoc/>
        public void StopListening()
        {
            _serviceCts?.Cancel();
            _connectionListener.StopListening();
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

        private void OnConnectionAccepted(object? sender, ConnectedDevice device)
        {
            var token = _serviceCts?.Token ?? CancellationToken.None;
            _ = Task.Run(() => HandleConnectionAsync(device, token));
        }

        private async Task HandleConnectionAsync(ConnectedDevice device, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var message = await device.ReceiveMessageAsync(cancellationToken);
                    var messageType = (MessageType)message[0];

                    switch (messageType)
                    {
                        case MessageType.PairingRequest:
                            await HandlePairingRequestAsync(device, message, cancellationToken);
                            CleanupSessionState();
                            break;

                        case MessageType.SecureSessionRequest:
                            await HandleSecureSessionRequestAsync(device, message, cancellationToken);
                            break;

                        case MessageType.SecureAuthRequest:
                            await HandleSecureAuthRequestAsync(device, message, cancellationToken);
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
                device.Dispose();
                CleanupSessionState();
                Disconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        #region Secure Pairing

        private async Task HandlePairingRequestAsync(ConnectedDevice device, byte[] message,
            CancellationToken cancellationToken)
        {
            // Parse desktop's ECDH public key
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var desktopName = reader.ReadString(); // Read desktop name
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
            var response = ProtocolSerializer.CreatePairingResponse(myPublicKey);
            await device.SendMessageAsync(response, cancellationToken);

            // Notify UI to display verification code and wait for user confirmation
            // This happens BEFORE we receive PairingConfirm from desktop
            _pairingConfirmationTcs = new TaskCompletionSource<bool>();

            // Show the pairing request with verification code
            PairingRequested?.Invoke(this, new PairingRequestViewModel(desktopName, string.Empty, verificationCode));
            VerificationCodeReady?.Invoke(this, verificationCode);

            // Wait for user confirmation on mobile (user confirms code matches)
            var userConfirmed = await _pairingConfirmationTcs.Task;
            if (!userConfirmed)
            {
                // We need to wait for desktop's message and reject it
                try
                {
                    _ = await device.ReceiveMessageAsync(cancellationToken);
                    await device.SendMessageAsync([(byte)MessageType.PairingRejected], cancellationToken);
                }
                catch (Exception)
                {
                    // Connection may have closed
                }

                CleanupSessionState();
                return;
            }

            // Now wait for pairing confirmation from desktop (which includes vault info)
            var confirmMessage = await device.ReceiveMessageAsync(cancellationToken);
            var confirmType = (MessageType)confirmMessage[0];

            if (confirmType == MessageType.PairingRejected)
            {
                CleanupSessionState();
                return;
            }

            if (confirmType != MessageType.PairingConfirm)
            {
                CleanupSessionState();
                return;
            }

            // Parse pairing confirmation
            ProtocolSerializer.ParsePairingConfirm(confirmMessage, out var credentialId, out var vaultName, out var pairingId, out var challenge);

            // Create and enroll credential with persistent challenge
            var credential = await CreateEnrolledCredentialAsync(credentialId, vaultName, desktopName, pairingId, challenge);

            // Get the encryption key to decrypt HMAC key for computing the initial HMAC
            var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(pairingId);
            if (encryptionKey == null)
            {
                await device.SendMessageAsync([(byte)MessageType.PairingRejected], cancellationToken);
                CleanupSessionState();
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
                var completeMessage = ProtocolSerializer.CreatePairingComplete(hmacResult);
                await device.SendMessageAsync(completeMessage, cancellationToken);

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
            string credentialId, string vaultName, string desktopName, string pairingId, byte[] challenge)
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
                desktopName ?? "Desktop",
                pairingId,
                challenge,
                _sharedSecret!);

            // Return the credential directly - it's been enrolled and added to the store
            return credential;
        }

        #endregion

        #region Secure Authentication

        private async Task HandleSecureSessionRequestAsync(ConnectedDevice device, byte[] message,
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
            var desktopEcdhPublicKey = reader.ReadBytes(keyLength);

            // Find credential by pairing ID
            var credential = _credentialStoreModel.GetByPairingId(pairingId);
            if (credential == null)
            {
                await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Unknown pairing"), cancellationToken);
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
            var response = ProtocolSerializer.CreateSecureSessionAccepted(mobileNonce, myPublicKey);
            await device.SendMessageAsync(response, cancellationToken);
        }

        private async Task HandleSecureAuthRequestAsync(ConnectedDevice device, byte[] message,
            CancellationToken cancellationToken)
        {
            if (_secureChannel == null || _currentCredential == null)
            {
                await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("No session"), cancellationToken);
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
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Request expired"), cancellationToken);
                    return;
                }

                // Verify credential matches
                if (_currentCredential.CredentialId != credentialId)
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Credential mismatch"), cancellationToken);
                    return;
                }

                // Verify the persistent challenge matches what we have stored
                var storedChallenge = _currentCredential.Challenge;
                if (storedChallenge == null || !persistentChallenge.SequenceEqual(storedChallenge))
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Challenge mismatch"), cancellationToken);
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
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("User rejected"), cancellationToken);
                    return;
                }

                // Decrypt HMAC key using stored encryption key
                var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(_currentCredential.PairingId);
                if (encryptionKey == null)
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Key error"), cancellationToken);
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
                    await device.SendMessageAsync(encryptedHmac, MessageType.SecureAuthResponse, cancellationToken);

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
            catch (CryptographicException)
            {
                await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Decryption failed"), cancellationToken);
            }
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

        #endregion

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


        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _connectionListener.ConnectionAccepted -= OnConnectionAccepted;
            StopListening();
            CleanupSessionState();
            _connectionListener.Dispose();
            _serviceCts?.Dispose();

            Disconnected = null;
            PairingRequested = null;
            EnrollmentCompleted = null;
            VerificationCodeReady = null;
            AuthenticationRequested = null;
            AuthenticationCompleted = null;
        }
    }
}
