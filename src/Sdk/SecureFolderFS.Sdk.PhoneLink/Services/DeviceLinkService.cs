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
using SecureFolderFS.Shared.Helpers;
using static SecureFolderFS.Sdk.PhoneLink.Constants;

namespace SecureFolderFS.Sdk.PhoneLink.Services
{
    /// <inheritdoc cref="IDeviceLinkService"/>
    public sealed class DeviceLinkService : IDeviceLinkService
    {
        private readonly CredentialsStoreModel _credentialStoreModel;
        private readonly DeviceConnectionListener _connectionListener;
        private readonly object _lock = new();
        private CancellationTokenSource? _serviceCts;
        private TaskCompletionSource<bool>? _pairingConfirmationTcs;
        private TaskCompletionSource<bool>? _authConfirmationTcs;
        private CredentialViewModel? _currentCredential;
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
            lock (_lock)
            {
                if (_disposed)
                    return Task.CompletedTask;

                SafetyHelpers.NoFailure(() => _serviceCts?.Dispose());
                _serviceCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            }

            return _connectionListener.StartListeningAsync(_serviceCts.Token);
        }

        /// <inheritdoc/>
        public void StopListening()
        {
            lock (_lock)
            {
                SafetyHelpers.NoFailure(() => _serviceCts?.Cancel());
                _connectionListener.StopListening();
            }
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
            CancellationToken token;
            lock (_lock)
            {
                if (_disposed || _serviceCts == null)
                {
                    device.Dispose();
                    return;
                }
                token = _serviceCts.Token;
            }

            _ = HandleConnectionAsync(device, token);
        }

        private async Task HandleConnectionAsync(ConnectedDevice device, CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested && device.IsConnected)
                {
                    byte[] message;
                    try
                    {
                        message = await device.ReceiveMessageAsync(cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        // Connection closed normally
                        break;
                    }

                    var messageType = (MessageType)message[0];

                    try
                    {
                        switch (messageType)
                        {
                            case MessageType.PairingRequest:
                                await HandlePairingRequestAsync(device, message, cancellationToken);
                                CleanupSessionState(); // Clean up after pairing completes
                                break;

                            case MessageType.SecureSessionRequest:
                                await HandleSecureSessionRequestAsync(device, message, cancellationToken);
                                // Don't clean up - session state needed for SecureAuthRequest
                                break;

                            case MessageType.SecureAuthRequest:
                                await HandleSecureAuthRequestAsync(device, message, cancellationToken);
                                CleanupSessionState(); // Clean up after authentication completes
                                break;

                            default:
                                Debug.WriteLine($"Unknown message type: {messageType}");
                                break;
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (IOException)
                    {
                        // Connection closed during operation
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error handling message {messageType}: {ex.Message}");
                        // Continue processing next message unless connection is broken
                        if (!device.IsConnected)
                            break;
                    }
                }
            }
            finally
            {
                device.Dispose();
                CleanupSessionState();
                SafetyHelpers.NoFailure(() => Disconnected?.Invoke(this, EventArgs.Empty));
            }
        }

        #region Secure Pairing

        private async Task HandlePairingRequestAsync(ConnectedDevice device, byte[] message, CancellationToken cancellationToken)
        {
            // Clean up any stale session state from previous operations
            CleanupSessionState();

            // Parse desktop's ECDH public key
            using var ms = new MemoryStream(message);
            using var reader = new BinaryReader(ms);

            reader.ReadByte(); // Skip message type
            var desktopName = reader.ReadString();
            var desktopType = reader.ReadString();
            var keyLength = reader.ReadInt32();
            var desktopEcdhPublicKey = reader.ReadBytes(keyLength);

            // Generate our ECDH keypair
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive shared secret BEFORE sending response (so we can show code immediately)
            var sharedSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, desktopEcdhPublicKey);

            try
            {
                // Compute verification code
                var verificationCode = SecureChannelModel.ComputeVerificationCode(sharedSecret);

                // Send our public key response
                var response = ProtocolSerializer.CreatePairingResponse(myPublicKey);
                await device.SendMessageAsync(response, cancellationToken);

                // Notify UI to display verification code and wait for user confirmation
                _pairingConfirmationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                // Show the pairing request with verification code
                PairingRequested?.Invoke(this, new PairingRequestViewModel(desktopName, desktopType, string.Empty, verificationCode));
                VerificationCodeReady?.Invoke(this, verificationCode);

                // Wait for user confirmation on mobile (user confirms code matches)
                bool userConfirmed;
                await using (cancellationToken.Register(() => _pairingConfirmationTcs.TrySetCanceled()))
                    userConfirmed = await _pairingConfirmationTcs.Task;

                if (!userConfirmed)
                {
                    // Wait for desktop's message and reject it
                    await SafetyHelpers.NoFailureAsync(async () =>
                    {
                        using var rejectCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, rejectCts.Token);
                        _ = await device.ReceiveMessageAsync(linkedCts.Token);
                        await device.SendMessageAsync([(byte)MessageType.PairingRejected], linkedCts.Token);
                    });
                    return;
                }

                // Now wait for pairing confirmation from desktop (which includes vault info)
                var confirmMessage = await device.ReceiveMessageAsync(cancellationToken);
                var confirmType = (MessageType)confirmMessage[0];

                if (confirmType == MessageType.PairingRejected)
                    return;

                if (confirmType != MessageType.PairingConfirm)
                    return;

                // Parse pairing confirmation
                ProtocolSerializer.ParsePairingConfirm(confirmMessage, out var credentialId, out var vaultName, out var pairingId, out var challenge);

                // Create and enroll credential with persistent challenge
                var credential = new CredentialViewModel()
                {
                    DisplayName = vaultName,
                    CreatedAt = DateTime.Now
                };
                await _credentialStoreModel.EnrollCredentialAsync(
                    credential,
                    credentialId,
                    vaultName,
                    desktopName,
                    desktopType,
                    pairingId,
                    challenge,
                    sharedSecret);

                // Get the encryption key to decrypt HMAC key for computing the initial HMAC
                var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(pairingId);
                if (encryptionKey == null)
                {
                    await device.SendMessageAsync([(byte)MessageType.PairingRejected], cancellationToken);
                    return;
                }

                try
                {
                    // Decrypt HMAC key so we can compute HMAC
                    credential.DecryptHmacKey(encryptionKey);

                    // Compute HMAC over the persistent challenge data
                    var challengeData = BuildChallengeData(credentialId, challenge);
                    var hmacResult = credential.ComputeHmac(challengeData);

                    // Send pairing complete with HMAC result
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
            finally
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
            }
        }

        #endregion

        #region Secure Authentication

        private async Task HandleSecureSessionRequestAsync(ConnectedDevice device, byte[] message,
            CancellationToken cancellationToken)
        {
            // Clean up any stale session state from previous operations
            CleanupSessionState();

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
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive session secret using ECDH (transport security only)
            var sharedSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, desktopEcdhPublicKey);

            // Generate our session nonce
            var mobileNonce = new byte[16];
            RandomNumberGenerator.Fill(mobileNonce);

            // Derive session key from ECDH secret + both nonces
            var combinedNonce = new byte[desktopNonce.Length + mobileNonce.Length];
            desktopNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, desktopNonce.Length);

            _secureChannel = new SecureChannelModel(sharedSecret, combinedNonce);

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
                var age = DateTimeOffset.Now - requestTime;

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
                _authConfirmationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var authInfo = new AuthenticationRequestModel(_currentCredential.VaultName, _currentCredential.MachineName, _currentCredential.MachineType, _currentCredential.DisplayName);

                // Notify UI about auth request (could require biometric)
                AuthenticationRequested?.Invoke(this, authInfo);

                // Wait for user to accept or reject
                bool userConfirmed;
                using (cancellationToken.Register(() => _authConfirmationTcs.TrySetCanceled()))
                {
                    userConfirmed = await _authConfirmationTcs.Task;
                }

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
                await SafetyHelpers.NoFailureAsync(async () =>
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Decryption failed"), cancellationToken));
            }
        }

        /// <summary>
        /// Builds the data for HMAC computation.
        /// Must match desktop's BuildChallengeData exactly.
        /// </summary>
        private static byte[] BuildChallengeData(string credentialId, byte[] persistentChallenge)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(Encoding.UTF8.GetBytes(credentialId));
            writer.Write(persistentChallenge);

            return ms.ToArray();
        }

        #endregion

        /// <summary>
        /// Cleans up cryptographic session state.
        /// </summary>
        private void CleanupSessionState()
        {
            SafetyHelpers.NoFailure(() => _secureChannel?.Dispose());
            _secureChannel = null;

            _pairingConfirmationTcs = null;
            _authConfirmationTcs = null;
            _currentCredential = null;
        }

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _connectionListener.ConnectionAccepted -= OnConnectionAccepted;
            StopListening();
            CleanupSessionState();
            _connectionListener.Dispose();

            SafetyHelpers.NoFailure(() => _serviceCts?.Dispose());

            Disconnected = null;
            PairingRequested = null;
            EnrollmentCompleted = null;
            VerificationCodeReady = null;
            AuthenticationRequested = null;
            AuthenticationCompleted = null;
        }
    }
}
