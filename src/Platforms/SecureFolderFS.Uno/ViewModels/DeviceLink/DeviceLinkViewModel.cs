using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.PhoneLink.Enums;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.PhoneLink.Results;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Uno.DataModels;

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
        /// Gets the name of the current machine.
        /// </summary>
        protected virtual string MachineName { get; }

        /// <inheritdoc/>
        public sealed override bool CanComplement { get; } = true;

        /// <inheritdoc/>
        public sealed override AuthenticationStage Availability { get; } = AuthenticationStage.Any;
        
        public DeviceLinkViewModel(IFolder vaultFolder, string vaultId)
            : base(Constants.Vault.Authentication.AUTH_DEVICE_LINK)
        {
            Title = "DeviceLink".ToLocalized();
            MachineName = Environment.MachineName;
            VaultName = vaultFolder.Name;
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }
        
        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            using var deviceDiscovery = new DeviceDiscovery();
            var devices = await deviceDiscovery.DiscoverDevicesAsync(cancellationToken: cancellationToken);
            var discoveredDevice = devices.FirstOrDefault();
            if (discoveredDevice is null)
                throw new InvalidOperationException("No device link devices found.");

            // Step 1: Connect to device
            using var connectedDevice = await ConnectedDevice.ConnectAsync(discoveredDevice, cancellationToken);
            
            // Step 2: Generate ECDH key pair
            using var ecdhKeyPair = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var publicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();
            
            // Step 3: Send pairing request
            var pairingRequest = ProtocolSerializer.CreatePairingRequest(MachineName, publicKey);
            await connectedDevice.SendMessageAsync(pairingRequest, cancellationToken);

            // Step 4: receive pairing response
            var response = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            var messageType = (MessageType)response[0];
            
            if (messageType == MessageType.PairingRejected)
                throw new UnauthorizedAccessException("Device link pairing was rejected by the remote device.");
            
            if (messageType != MessageType.PairingResponse)
                throw new InvalidOperationException("Unexpected response received during device link enrollment.");
            
            // Step 5: Derive session secret
            var mobileEcdhPublicKey = ProtocolSerializer.ParsePairingResponse(response);
            var sharedSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, mobileEcdhPublicKey);

            // Step 6: Compute and display verification code
            var verificationCode = SecureChannelModel.ComputeVerificationCode(sharedSecret);
            
            // Step 7: User confirms the code matches
            var codeConfirmed = await ShowVerificationCodeAsync(verificationCode);
            if (!codeConfirmed)
                throw new UnauthorizedAccessException("Device link pairing was rejected by the user.");

            // Step 8: Generate CID, PairingID
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
            return new DeviceLinkPairingResult(ManagedKey.TakeOwnership(initialHmac))
            {
                CredentialId = credentialId,
                PairingId = pairingId,
                MobileDeviceId = discoveredDevice.DeviceId,
                MobileDeviceName = discoveredDevice.DeviceName
            };
        }

        /// <inheritdoc/>
        public override async Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            var dataModel = await GetConfigurationAsync(cancellationToken);
            using var deviceDiscovery = new DeviceDiscovery();

            var tries = 0;
            while (!cancellationToken.IsCancellationRequested)
            {
                tries++;
                var devices = await deviceDiscovery.DiscoverDevicesAsync(cancellationToken: cancellationToken);
                var discoveredDevice = devices.FirstOrDefault(d => d.DeviceId == dataModel.MobileDeviceId);

                if (discoveredDevice is not null)
                    return await AuthenticateMobileAsync(discoveredDevice, dataModel, cancellationToken);

                await Task.Delay(2000, cancellationToken);
                if (tries >= 3)
                    break;
            }

            throw new TimeoutException("Timed out waiting for paired device.");
        }

        private async Task<DeviceLinkAuthenticationResult> AuthenticateMobileAsync(DiscoveredDevice device, VaultDeviceLinkDataModel dataModel, CancellationToken cancellationToken)
        {
            using var connectedDevice = await ConnectedDevice.ConnectAsync(device, cancellationToken);
            
            // Generate fresh ECDH keypair for this session (transport security)
            using var ecdhKeyPair = SecureChannelModel.GenerateKeyPair();
            var myPublicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();

            // Generate session nonce
            var sessionNonce = new byte[16];
            RandomNumberGenerator.Fill(sessionNonce);

            // Send secure session request with our ECDH public key
            var sessionRequest = ProtocolSerializer.CreateSecureSessionRequest(dataModel.PairingId, sessionNonce, myPublicKey);
            await connectedDevice.SendMessageAsync(sessionRequest, cancellationToken);

            // Receive response
            var response = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            var messageType = (MessageType)response[0];

            if (messageType != MessageType.SecureSessionAccepted)
                throw new InvalidOperationException("Unexpected response received during secure session establishment.");

            ProtocolSerializer.ParseSecureSessionAccepted(response, out var mobileNonce, out var mobileEcdhPublicKey);

            // Derive session secret using ECDH (ephemeral, for transport only)
            var sharedSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, mobileEcdhPublicKey);

            // Derive session key from ECDH secret + both nonces
            var combinedNonce = new byte[sessionNonce.Length + mobileNonce.Length];
            sessionNonce.CopyTo(combinedNonce, 0);
            mobileNonce.CopyTo(combinedNonce, sessionNonce.Length);

            using var secureChannel = new SecureChannelModel(sharedSecret, combinedNonce);
            
            // Step 3: Send authentication request with PERSISTENT CHALLENGE
            // The persistent challenge is the same every time - mobile signs it
            var persistentChallenge = dataModel.Challenge;
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            
            var authRequest = ProtocolSerializer.CreateSecureAuthRequest(dataModel.CredentialId, persistentChallenge, timestamp);
            var encryptedRequest = secureChannel.Encrypt(authRequest);
            await connectedDevice.SendMessageAsync(encryptedRequest, MessageType.SecureAuthRequest, cancellationToken);

            // Step 4: Receive encrypted response
            var encryptedResponse = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            messageType = (MessageType)encryptedResponse[0];

            if (messageType == MessageType.AuthenticationRejected)
                throw new UnauthorizedAccessException("Authentication was rejected by the remote device.");

            if (messageType != MessageType.SecureAuthResponse)
                throw new InvalidOperationException("Unexpected response received during authentication.");

            // Decrypt response
            var responsePayload = encryptedResponse.AsSpan(1).ToArray();
            var decryptedResponse = secureChannel.Decrypt(responsePayload);
            var receivedHmac = decryptedResponse;

            // Step 5: Verify HMAC matches expected value
            var expectedHmac = dataModel.ExpectedHmac;
            var isValid = CryptographicOperations.FixedTimeEquals(receivedHmac, expectedHmac);

            if (!isValid)
                throw new CryptographicException("HMAC verification failed.");
            
            var managedHmac = ManagedKey.TakeOwnership(receivedHmac);
            return new DeviceLinkAuthenticationResult(managedHmac)
            {
                CredentialId = dataModel.CredentialId
            };
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
