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
#pragma warning disable SYSLIB5006

namespace SecureFolderFS.Sdk.DeviceLink.Services
{
    /// <inheritdoc cref="IDeviceLinkService"/>
    public sealed class DeviceLinkService : IDeviceLinkService
    {
        private readonly CredentialsStoreModel _credentialStoreModel;
        private readonly DeviceConnectionListener _connectionListener;
        private readonly Lock _lock = new();
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
            // Parse desktop's ECDH and ML-KEM public keys
            ProtocolSerializer.ParsePairingRequest(
                message,
                out var desktopName,
                out var desktopType,
                out var desktopEcdhPublicKey,
                out var desktopMlKemPublicKey);

            // Generate our ECDH keypair (classical, for forward secrecy)
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myEcdhPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive classical ECDH component
            var ecdhSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, desktopEcdhPublicKey);

            // Encapsulate against desktop's ML-KEM public key.
            // This produces a ciphertext (sent to desktop) and a shared secret (kept here).
            // Only the desktop, holding the ML-KEM private key, can decapsulate to get the same secret.
            using var desktopMlKemKey = MLKem.ImportSubjectPublicKeyInfo(desktopMlKemPublicKey);
            desktopMlKemKey.Encapsulate(out var mlKemCiphertext, out var mlKemSecret);

            byte[] sharedSecret;
            try
            {
                sharedSecret = SecureChannelModel.CombineHybridSecrets(ecdhSecret, mlKemSecret);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(ecdhSecret);
                CryptographicOperations.ZeroMemory(mlKemSecret);
            }

            try
            {
                // Compute verification code from the hybrid shared secret
                var verificationCode = SecureChannelModel.ComputeVerificationCode(sharedSecret);

                // Send our ECDH public key and the ML-KEM ciphertext
                var response = ProtocolSerializer.CreatePairingResponse(myEcdhPublicKey, mlKemCiphertext);
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

                // The verification code confirmed the hybrid key exchange is free of an active MITM,
                // so encrypt all subsequent pairing messages under the trusted shared secret. The
                // confirm carries the persistent challenge and the complete carries the HMAC key
                // contribution - neither may travel in cleartext.
                using var pairingChannel = new SecureChannelModel(sharedSecret);

                // Now wait for pairing confirmation from desktop (which includes vault info)
                var confirmMessage = await device.ReceiveMessageAsync(cancellationToken);
                var confirmType = (MessageType)confirmMessage[0];

                if (confirmType == MessageType.PairingRejected)
                    return;

                if (confirmType != MessageType.PairingConfirm)
                    return;

                // Decrypt and parse pairing confirmation
                var confirmPayload = confirmMessage.AsSpan(Constants.KeyTraits.MESSAGE_BYTE_LENGTH);
                var decryptedConfirm = pairingChannel.Decrypt(confirmPayload);
                ProtocolSerializer.ParsePairingConfirm(decryptedConfirm, out var credentialId, out var vaultName, out var pairingId, out var challenge);

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
                    // Decrypt HMAC key so we can compute the derived values
                    credential.DecryptHmacKey(encryptionKey);

                    // Two independent, domain-separated derivations of the HMAC key:
                    // 1. The key contribution feeds the vault's key derivation on the desktop and is
                    //    never persisted there (the desktop keeps only a hash to verify responses).
                    // 2. The binding secret authenticates future session channels and is the only
                    //    secret the desktop stores at rest. Knowing it does not reveal the key contribution.
                    var keyContribution = credential.ComputeHmac(BuildChallengeData(credentialId, challenge));
                    var bindingSecret = credential.ComputeHmac(BuildBindingData(credentialId, challenge));

                    // Send pairing complete with both values (encrypted over the pairing channel)
                    var completeMessage = ProtocolSerializer.CreatePairingComplete(keyContribution, bindingSecret);
                    var encryptedComplete = pairingChannel.Encrypt(completeMessage);
                    await device.SendMessageAsync(encryptedComplete, MessageType.PairingComplete, cancellationToken);

                    // Clear the decrypted key from memory
                    credential.ClearDecryptedKey();
                    CryptographicOperations.ZeroMemory(keyContribution);
                    CryptographicOperations.ZeroMemory(bindingSecret);

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
            // Parse the desktop's pairingId, nonce, ECDH public key, and ML-KEM public key
            ProtocolSerializer.ParseSecureSessionRequest(
                message,
                out var pairingId,
                out var desktopNonce,
                out var desktopEcdhPublicKey,
                out var desktopMlKemPublicKey);

            // Find credential by pairing ID
            var credential = _credentialStoreModel.GetByPairingId(pairingId);
            if (credential == null)
            {
                await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Unknown pairing"), cancellationToken);
                return;
            }

            // Store credential in session for later use in auth request
            session.CurrentCredential = credential;

            // Generate fresh ephemeral ECDH keypair for this session (transport security)
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myEcdhPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Derive classical ECDH component
            var ecdhSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, desktopEcdhPublicKey);

            // Encapsulate against desktop's ML-KEM public key for this session.
            // Fresh encapsulation per session ensures PQC forward secrecy.
            using var desktopMlKemKey = MLKem.ImportSubjectPublicKeyInfo(desktopMlKemPublicKey);
            desktopMlKemKey.Encapsulate(out var mlKemCiphertext, out var mlKemSecret);

            byte[] sharedSecret;
            try
            {
                sharedSecret = SecureChannelModel.CombineHybridSecrets(ecdhSecret, mlKemSecret);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(ecdhSecret);
                CryptographicOperations.ZeroMemory(mlKemSecret);
            }

            // Generate our session nonce
            var mobileNonce = new byte[16];
            RandomNumberGenerator.Fill(mobileNonce);

            // Derive session key from hybrid shared secret + both nonces
            var combinedNonce = new byte[desktopNonce.Length + mobileNonce.Length];
            desktopNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, desktopNonce.Length);

            // Compute the pairing binding secret (== the desktop's stored BindingSecret) and fold it
            // into the channel key. Only a device that holds this credential's HMAC key can reproduce
            // it, which authenticates this otherwise-anonymous ephemeral handshake to the established
            // pairing: an active MITM or a device-id impersonator that lacks the secret derives a
            // different key and is rejected by AES-GCM authentication, so it can neither read the
            // challenge nor capture the key contribution in the response.
            var encryptionKey = await _credentialStoreModel.GetEncryptionKeyAsync(pairingId);
            if (encryptionKey is null || credential.CredentialId is null || credential.Challenge is null)
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
                if (encryptionKey is not null)
                    CryptographicOperations.ZeroMemory(encryptionKey);

                await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Key error"), cancellationToken);
                return;
            }

            byte[]? bindingSecret = null;
            try
            {
                credential.DecryptHmacKey(encryptionKey);
                var bindingData = BuildBindingData(credential.CredentialId, credential.Challenge);
                bindingSecret = credential.ComputeHmac(bindingData);

                session.SecureChannel = new SecureChannelModel(sharedSecret, combinedNonce, bindingSecret);
            }
            finally
            {
                credential.ClearDecryptedKey();
                CryptographicOperations.ZeroMemory(encryptionKey);
                CryptographicOperations.ZeroMemory(sharedSecret);
                if (bindingSecret is not null)
                    CryptographicOperations.ZeroMemory(bindingSecret);
            }

            // Send response with our ECDH public key and ML-KEM ciphertext
            var response = ProtocolSerializer.CreateSecureSessionAccepted(mobileNonce, myEcdhPublicKey, mlKemCiphertext);
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

                // Parse request (persistent challenge + fresh request nonce from desktop)
                ProtocolSerializer.ParseSecureAuthRequest(decryptedPayload, out var credentialId, out var persistentChallenge, out var timestamp, out var requestNonce);

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
                if (storedChallenge == null || !CryptographicOperations.FixedTimeEquals(persistentChallenge, storedChallenge))
                {
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Challenge mismatch"), cancellationToken);
                    return;
                }

                // Create a TaskCompletionSource for user confirmation
                session.AuthConfirmationTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                var authInfo = new AuthenticationRequestViewModel(session.AuthConfirmationTcs)
                {
                    VaultName = session.CurrentCredential.VaultName,
                    DesktopName = session.CurrentCredential.DesktopName,
                    DesktopType = session.CurrentCredential.DesktopType,
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

                byte[]? keyContribution = null;
                try
                {
                    session.CurrentCredential.DecryptHmacKey(encryptionKey);

                    // Compute the vault key contribution and echo the desktop's request nonce.
                    // The echoed nonce proves this response was produced for this exact request.
                    keyContribution = session.CurrentCredential.ComputeHmac(BuildChallengeData(credentialId, persistentChallenge));
                    var responsePayload = ProtocolSerializer.CreateSecureAuthResponse(keyContribution, requestNonce);

                    // Encrypt and send response
                    var encryptedResponse = session.SecureChannel.Encrypt(responsePayload);
                    await device.SendMessageAsync(encryptedResponse, MessageType.SecureAuthResponse, cancellationToken);

                    // Notify UI that authentication completed successfully
                    AuthenticationCompleted?.Invoke(this, EventArgs.Empty);
                }
                finally
                {
                    // Clear decrypted key material from memory
                    session.CurrentCredential.ClearDecryptedKey();
                    CryptographicOperations.ZeroMemory(encryptionKey);
                    if (keyContribution is not null)
                        CryptographicOperations.ZeroMemory(keyContribution);
                }
            }
            catch (CryptographicException)
            {
                await SafetyHelpers.NoFailureAsync(async () =>
                    await device.SendMessageAsync(ProtocolSerializer.CreateAuthenticationRejected("Decryption failed"), cancellationToken));
            }
        }

        /// <summary>
        /// Builds the HMAC input for the vault key contribution.
        /// </summary>
        private static byte[] BuildChallengeData(string credentialId, byte[] persistentChallenge)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write(Encoding.UTF8.GetBytes(credentialId));
            writer.Write(persistentChallenge);

            return ms.ToArray();
        }

        /// <summary>
        /// Builds the HMAC input for the channel binding secret. The domain-separation label keeps it
        /// cryptographically independent from the vault key contribution: the desktop persists the
        /// binding secret at rest, and knowing it must not reveal the key contribution.
        /// </summary>
        private static byte[] BuildBindingData(string credentialId, byte[] persistentChallenge)
        {
            using var ms = new MemoryStream();
            using var writer = new BinaryWriter(ms);

            writer.Write("DeviceLink-Binding-v1"u8);
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