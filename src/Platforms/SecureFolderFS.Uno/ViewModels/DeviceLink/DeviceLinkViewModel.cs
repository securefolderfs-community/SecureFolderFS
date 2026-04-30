using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Sdk.DeviceLink.Enums;
using SecureFolderFS.Sdk.DeviceLink.Models;
using SecureFolderFS.Sdk.DeviceLink.Results;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.SecureStore;
using SecureFolderFS.Storage.Extensions;
using SecureFolderFS.Uno.DataModels;

#pragma warning disable SYSLIB5006

namespace SecureFolderFS.Uno.ViewModels.DeviceLink
{
    [Bindable(true)]
    public abstract class DeviceLinkViewModel : AuthenticationViewModel
    {
        /// <summary>
        /// Gets the unique ID of the vault.
        /// </summary>
        protected string VaultId { get; }

        /// <summary>
        /// Gets the associated folder of the vault.
        /// </summary>
        protected IFolder VaultFolder { get; }

        /// <summary>
        /// Gets the name of the vault.
        /// </summary>
        protected virtual string VaultName { get; }

        /// <summary>
        /// Gets the name of the desktop machine.
        /// </summary>
        protected virtual string DesktopName { get; }

        /// <summary>
        /// Gets the type of the desktop machine, typically used to identify the kind of device for pairing processes.
        /// </summary>
        protected virtual string DesktopType { get; }

        /// <inheritdoc/>
        public sealed override bool CanComplement { get; } = true;

        /// <inheritdoc/>
        public sealed override AuthenticationStage Availability { get; } = AuthenticationStage.Any;

        public DeviceLinkViewModel(IFolder vaultFolder, string vaultId)
            : base(Constants.Vault.Authentication.AUTH_DEVICE_LINK)
        {
            Title = "DeviceLink".ToLocalized();
            DesktopName = Environment.MachineName;
            DesktopType = DeviceDiscovery.GetDeviceType();
            VaultName = vaultFolder.Name;
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }

        /// <inheritdoc/>
        public override async Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            if (VaultFolder is not IModifiableFolder modifiableFolder)
                return;

            var authenticationFile = await modifiableFolder.TryGetFileByNameAsync($"{Id}{Constants.Vault.Names.CONFIGURATION_EXTENSION}", cancellationToken);
            if (authenticationFile is null)
                return;

            await modifiableFolder.DeleteAsync(authenticationFile, cancellationToken);
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);

            using var deviceDiscovery = new DeviceDiscovery(DesktopName);
            var devices = await deviceDiscovery.DiscoverDevicesAsync(cancellationToken: cancellationToken);
            var discoveredDevice = devices.FirstOrDefault();
            if (discoveredDevice is null)
                throw new InvalidOperationException("No device link devices found.");

            // Step 1: Connect to device
            using var connectedDevice = await ConnectedDevice.ConnectAsync(discoveredDevice, cancellationToken);

            // Step 2: Generate hybrid key pairs.
            //   ECDH provides classical forward secrecy.
            //   ML-KEM allows the mobile to encapsulate a PQC shared secret that only we can decapsulate.
            using var ecdhKeyPair = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            using var mlKemKeyPair = SecureChannelModel.GenerateMlKemKeyPair();

            var ecdhPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();
            var mlKemPublicKey = mlKemKeyPair.ExportSubjectPublicKeyInfo();

            // Step 3: Send pairing request with both public keys
            var pairingRequest = ProtocolSerializer.CreatePairingRequest(DesktopName, DesktopType, ecdhPublicKey, mlKemPublicKey);
            await connectedDevice.SendMessageAsync(pairingRequest, cancellationToken);

            // Step 4: Receive pairing response
            var response = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            var messageType = (MessageType)response[0];

            if (messageType == MessageType.PairingRejected)
                throw new UnauthorizedAccessException("Device link pairing was rejected by the remote device.");

            if (messageType != MessageType.PairingResponse)
                throw new InvalidOperationException("Unexpected response received during device link enrollment.");

            // Step 5: Derive hybrid shared secret.
            //   Classical: ECDH with mobile's ephemeral public key.
            //   PQC: decapsulate the ML-KEM ciphertext the mobile encapsulated for us.
            //   Both must be correct to reproduce the same shared secret.
            ProtocolSerializer.ParsePairingResponse(response, out var mobileEcdhPublicKey, out var mlKemCiphertext);

            var ecdhSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, mobileEcdhPublicKey);
            var mlKemSecret = mlKemKeyPair.Decapsulate(mlKemCiphertext);

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
                // Step 6: Compute and display verification code from the hybrid shared secret.
                //   Both sides derive this identically, so a mismatch means the connection was tampered with.
                var verificationCode = SecureChannelModel.ComputeVerificationCode(sharedSecret);

                // Step 7: User confirms the code matches on both devices
                var codeConfirmed = await ShowVerificationCodeAsync(verificationCode);
                if (!codeConfirmed)
                    throw new UnauthorizedAccessException("Device link pairing was rejected by the user.");

                // Step 8: Generate IDs here so they're in scope for the returned result,
                //   then send the confirmation message to mobile.
                var pairingId = Guid.NewGuid().ToString();
                var credentialId = Guid.NewGuid().ToString();

                var confirmationMessage = ProtocolSerializer.CreatePairingConfirmMessage(credentialId, VaultName, pairingId, data);
                await connectedDevice.SendMessageAsync(confirmationMessage, cancellationToken);

                // Step 9: Receive pairing complete with initial HMAC
                var completeResponse = await connectedDevice.ReceiveMessageAsync(cancellationToken);

                messageType = (MessageType)completeResponse[0];
                if (messageType != MessageType.PairingComplete)
                    throw new InvalidOperationException("Unexpected response received during device link enrollment.");

                var initialHmac = ProtocolSerializer.ParsePairingComplete(completeResponse);

                // Return all pairing metadata so DeviceLinkCreationViewModel can persist it
                return new DeviceLinkPairingResult(ManagedKey.TakeOwnership(initialHmac))
                {
                    CredentialId = credentialId,
                    PairingId = pairingId,
                    MobileDeviceId = discoveredDevice.DeviceId,
                    MobileDeviceName = discoveredDevice.DeviceName,
                    MobileDeviceType = discoveredDevice.DeviceType,
                };
            }
            finally
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
            }
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            // Use shorter discovery timeout for faster retries
            const int DISCOVERY_TIMEOUT_MS = 1500;
            const int RETRY_DELAY_MS = 500;
            const int MAX_TRIES = 5;

            var dataModel = await GetConfigurationAsync(cancellationToken);
            using var deviceDiscovery = new DeviceDiscovery(DesktopName);

            for (var tries = 0; tries < MAX_TRIES && !cancellationToken.IsCancellationRequested; tries++)
            {
                var devices = await deviceDiscovery.DiscoverDevicesAsync(DISCOVERY_TIMEOUT_MS, cancellationToken);
                var discoveredDevice = devices.FirstOrDefault(d => d.DeviceId == dataModel.MobileDeviceId);

                if (discoveredDevice is not null)
                {
                    try
                    {
                        return await AuthenticateMobileAsync(discoveredDevice, dataModel, cancellationToken);
                    }
                    catch (IOException) when (tries < MAX_TRIES - 1)
                    {
                        await Task.Delay(RETRY_DELAY_MS, cancellationToken);
                        continue;
                    }
                    catch (SocketException) when (tries < MAX_TRIES - 1)
                    {
                        await Task.Delay(RETRY_DELAY_MS, cancellationToken);
                        continue;
                    }
                }

                if (tries < MAX_TRIES - 1)
                    await Task.Delay(RETRY_DELAY_MS, cancellationToken);
            }

            throw new TimeoutException("Timed out waiting for paired device.");
        }

        private async Task<DeviceLinkAuthenticationResult> AuthenticateMobileAsync(DiscoveredDevice device, VaultDeviceLinkDataModel dataModel, CancellationToken cancellationToken)
        {
            using var connectedDevice = await ConnectedDevice.ConnectAsync(device, cancellationToken);

            // Generate fresh ephemeral hybrid key pairs for this session.
            // A new ML-KEM keypair per session ensures PQC forward secrecy:
            // compromising one session's keys reveals nothing about any other session.
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            using var mlKemKeyPair = SecureChannelModel.GenerateMlKemKeyPair();

            var myEcdhPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();
            var myMlKemPublicKey = mlKemKeyPair.ExportSubjectPublicKeyInfo();

            // Generate session nonce
            var sessionNonce = new byte[16];
            RandomNumberGenerator.Fill(sessionNonce);

            // Send secure session request with both public keys
            var sessionRequest = ProtocolSerializer.CreateSecureSessionRequest(dataModel.PairingId, sessionNonce, myEcdhPublicKey, myMlKemPublicKey);
            await connectedDevice.SendMessageAsync(sessionRequest, cancellationToken);

            // Receive response
            var response = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            var messageType = (MessageType)response[0];

            if (messageType != MessageType.SecureSessionAccepted)
                throw new InvalidOperationException("Unexpected response received during secure session establishment.");

            // Parse mobile's ECDH public key and ML-KEM ciphertext
            ProtocolSerializer.ParseSecureSessionAccepted(response, out var mobileNonce, out var mobileEcdhPublicKey, out var mlKemCiphertext);

            // Derive hybrid session secret
            var ecdhSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, mobileEcdhPublicKey);
            var mlKemSecret = mlKemKeyPair.Decapsulate(mlKemCiphertext);

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

            // Derive session key from hybrid secret + both nonces
            var combinedNonce = new byte[sessionNonce.Length + mobileNonce.Length];
            sessionNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, sessionNonce.Length);

            SecureChannelModel secureChannel;
            try
            {
                secureChannel = new SecureChannelModel(sharedSecret, combinedNonce);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(sharedSecret);
            }

            using (secureChannel)
            {
                // Send authentication request encrypted over the hybrid-secured channel
                var timestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
                var authRequest = ProtocolSerializer.CreateSecureAuthRequest(dataModel.CredentialId, dataModel.Challenge, timestamp);
                var encryptedRequest = secureChannel.Encrypt(authRequest);
                await connectedDevice.SendMessageAsync(encryptedRequest, MessageType.SecureAuthRequest, cancellationToken);

                // Receive encrypted response
                var encryptedResponse = await connectedDevice.ReceiveMessageAsync(cancellationToken);
                messageType = (MessageType)encryptedResponse[0];

                if (messageType == MessageType.AuthenticationRejected)
                    throw new UnauthorizedAccessException("Authentication was rejected by the remote device.");

                if (messageType != MessageType.SecureAuthResponse)
                    throw new InvalidOperationException("Unexpected response received during authentication.");

                // Decrypt response
                var responsePayload = encryptedResponse.AsSpan(Sdk.DeviceLink.Constants.KeyTraits.MESSAGE_BYTE_LENGTH);
                var decryptedHmac = secureChannel.Decrypt(responsePayload);

                // Verify HMAC matches expected value
                if (!CryptographicOperations.FixedTimeEquals(decryptedHmac, dataModel.ExpectedHmac))
                    throw new CryptographicException("HMAC verification failed.");

                return new DeviceLinkAuthenticationResult(ManagedKey.TakeOwnership(decryptedHmac))
                {
                    CredentialId = dataModel.CredentialId
                };
            }
        }

        /// <summary>
        /// Fetches the configuration data associated with the vault.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <see cref="VaultDeviceLinkDataModel"/> containing configuration data.</returns>
        protected abstract Task<VaultDeviceLinkDataModel> GetConfigurationAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Displays the verification code to the user for validation or acknowledgment.
        /// </summary>
        /// <param name="code">The verification code to be displayed.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <c>true</c> if the code was confirmed; otherwise, if rejected, <c>false</c>.</returns>
        protected abstract Task<bool> ShowVerificationCodeAsync(string code);
    }
}
