using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.DeviceLink.Enums;
using SecureFolderFS.Sdk.DeviceLink.Models;
using SecureFolderFS.Sdk.DeviceLink.ViewModels;
using SecureFolderFS.Shared.Helpers;
using static SecureFolderFS.Sdk.DeviceLink.Constants;

namespace SecureFolderFS.Sdk.DeviceLink.Services
{
    /// <inheritdoc cref="IDeviceLinkService"/>
    public sealed class DeviceLinkService : IDeviceLinkService
    {
        private readonly CredentialsStoreModel _credentialStoreModel;
        private readonly DeviceConnectionListener _connectionListener;
        private readonly object _lock = new();
        private CancellationTokenSource? _serviceCts;
        private CancellationToken _externalToken;
        private bool _disposed;

        public DeviceLinkService(string deviceName, string deviceId, CredentialsStoreModel credentialStoreModel)
        {
            _credentialStoreModel = credentialStoreModel;
            _connectionListener = new DeviceConnectionListener(deviceId, deviceName, DeviceDiscovery.GetDeviceType());
            _connectionListener.ConnectionAccepted += OnConnectionAccepted;
            _connectionListener.RestartRequested += OnRestartRequested;
        }

        /// <inheritdoc/>
        public event EventHandler<CredentialViewModel>? EnrollmentCompleted;

        /// <inheritdoc/>
        public event EventHandler<PairingRequestViewModel>? PairingRequested;

        /// <inheritdoc/>
        public event EventHandler<AuthenticationRequestViewModel>? AuthenticationRequested;

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

                _externalToken = cancellationToken;
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

        private async void OnRestartRequested(object? sender, EventArgs e)
        {
            if (_disposed || _externalToken.IsCancellationRequested)
                return;

            // Stop and restart the listener automatically
            StopListening();
            await Task.Delay(100).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);

            if (!_disposed && !_externalToken.IsCancellationRequested)
                await StartListeningAsync(_externalToken);
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
            // Create per-connection session state
            var session = new ConnectionSession();

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
                                await HandlePairingRequestAsync(device, session, message, cancellationToken);
                                session.Cleanup(); // Clean up after pairing completes
                                break;

                            case MessageType.SecureSessionRequest:
                                await HandleSecureSessionRequestAsync(device, session, message, cancellationToken);
                                // Don't clean up - session state needed for SecureAuthRequest
                                break;

                            case MessageType.SecureAuthRequest:
                                await HandleSecureAuthRequestAsync(device, session, message, cancellationToken);
                                session.Cleanup(); // Clean up after authentication completes
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
                    catch (Exception)
                    {
                        // Continue processing next message unless connection is broken
                        if (!device.IsConnected)
                            break;
                    }
                }
            }
            finally
            {
                device.Dispose();
                session.Cleanup();
                SafetyHelpers.NoFailure(() => Disconnected?.Invoke(this, EventArgs.Empty));
            }
        }

        #region Secure Pairing

        private async Task HandlePairingRequestAsync(ConnectedDevice device, ConnectionSession session, byte[] message, CancellationToken cancellationToken)
        {
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
                session.PairingConfirmationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

                // Show the pairing request with verification code
                PairingRequested?.Invoke(this, new PairingRequestViewModel(desktopName, desktopType, string.Empty, verificationCode, session.PairingConfirmationTcs));
                VerificationCodeReady?.Invoke(this, verificationCode);

                // Wait for user confirmation on mobile (user confirms code matches)
                bool userConfirmed;
                await using (cancellationToken.Register(() => session.PairingConfirmationTcs.TrySetCanceled()))
                    userConfirmed = await session.PairingConfirmationTcs.Task;

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

        private async Task HandleSecureSessionRequestAsync(ConnectedDevice device, ConnectionSession session, byte[] message,
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

            // Store credential in session for later use in auth request
            session.CurrentCredential = credential;

            // Generate fresh ECDH keypair for this session (ephemeral)
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var publicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive session secret using ECDH (transport security only)
            var sharedSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, desktopEcdhPublicKey);

            // Generate our session nonce
            var mobileNonce = new byte[16];
            RandomNumberGenerator.Fill(mobileNonce);

            // Derive session key from ECDH secret + both nonces
            var combinedNonce = new byte[desktopNonce.Length + mobileNonce.Length];
            desktopNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, desktopNonce.Length);

            session.SecureChannel = new SecureChannelModel(sharedSecret, combinedNonce);

            // Send response with our ECDH public key
            var response = ProtocolSerializer.CreateSecureSessionAccepted(mobileNonce, publicKey);
            await device.SendMessageAsync(response, cancellationToken);
        }

        private async Task HandleSecureAuthRequestAsync(ConnectedDevice device, ConnectionSession session, byte[] message,
            CancellationToken cancellationToken)
        {
            if (session.SecureChannel == null || session.CurrentCredential == null)
            {
                await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("No session"), cancellationToken);
                return;
            }

            try
            {
                // Decrypt request
                var encryptedPayload = message.AsSpan(Constants.KeyTraits.MESSAGE_BYTE_LENGTH);
                var decryptedPayload = session.SecureChannel.Decrypt(encryptedPayload);

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
                if (session.CurrentCredential.CredentialId != credentialId)
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Credential mismatch"), cancellationToken);
                    return;
                }

                // Verify the persistent challenge matches what we have stored
                var storedChallenge = session.CurrentCredential.Challenge;
                if (storedChallenge == null || !persistentChallenge.SequenceEqual(storedChallenge))
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Challenge mismatch"), cancellationToken);
                    return;
                }

                // Create a TaskCompletionSource for user confirmation
                session.AuthConfirmationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var authInfo = new AuthenticationRequestViewModel(session.AuthConfirmationTcs)
                {
                    VaultName = session.CurrentCredential.VaultName,
                    DesktopName = session.CurrentCredential.MachineName,
                    DesktopType = session.CurrentCredential.MachineType,
                    CredentialId = session.CurrentCredential.CredentialId,
                    CredentialName = session.CurrentCredential.DisplayName
                };

                // Notify UI about auth request (could require biometric)
                AuthenticationRequested?.Invoke(this, authInfo);

                // Wait for user to accept or reject
                bool userConfirmed;
                await using (cancellationToken.Register(() => session.AuthConfirmationTcs.TrySetCanceled()))
                    userConfirmed = await session.AuthConfirmationTcs.Task;

                if (!userConfirmed)
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("User rejected"), cancellationToken);
                    return;
                }

                // Decrypt HMAC key using stored encryption key
                var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(session.CurrentCredential.PairingId);
                if (encryptionKey == null)
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Key error"), cancellationToken);
                    return;
                }

                try
                {
                    session.CurrentCredential.DecryptHmacKey(encryptionKey);

                    // Compute HMAC over the challenge data
                    var challengeData = BuildChallengeData(credentialId, persistentChallenge);
                    var hmacResult = session.CurrentCredential.ComputeHmac(challengeData);

                    // Encrypt and send response
                    var encryptedHmac = session.SecureChannel.Encrypt(hmacResult);
                    await device.SendMessageAsync(encryptedHmac, MessageType.SecureAuthResponse, cancellationToken);

                    // Notify UI that authentication completed successfully
                    AuthenticationCompleted?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    // Clear decrypted key from memory
                    session.CurrentCredential.ClearDecryptedKey();
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

        public void Dispose()
        {
            lock (_lock)
            {
                if (_disposed)
                    return;

                _disposed = true;
            }

            _connectionListener.ConnectionAccepted -= OnConnectionAccepted;
            _connectionListener.RestartRequested -= OnRestartRequested;
            StopListening();
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
