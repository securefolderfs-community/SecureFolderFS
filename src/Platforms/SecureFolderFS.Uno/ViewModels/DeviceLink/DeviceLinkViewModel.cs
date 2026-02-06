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

namespace SecureFolderFS.Uno.ViewModels.DeviceLink
{
    [Bindable(true)]
    public abstract partial class DeviceLinkViewModel : AuthenticationViewModel
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
            using var deviceDiscovery = new DeviceDiscovery();
            var devices = await deviceDiscovery.DiscoverDevicesAsync(cancellationToken: cancellationToken);
            var discoveredDevice = devices.FirstOrDefault();
            if (discoveredDevice is null)
                throw new InvalidOperationException("No device link devices found.");

            using var connectedDevice = await ConnectedDevice.ConnectAsync(discoveredDevice, cancellationToken);
            var ecdhKeyPair = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
            var publicKey = ecdhKeyPair.ExportSubjectPublicKeyInfo();
            var pairingRequest = ProtocolSerializer.CreatePairingRequest(MachineName, publicKey);

            await connectedDevice.SendMessageAsync(pairingRequest, cancellationToken);
            var response = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            var messageType = (MessageType)response[0];
            
            if (messageType == MessageType.PairingRejected)
                throw new UnauthorizedAccessException("Device link pairing was rejected by the remote device.");
            
            if (messageType != MessageType.PairingResponse)
                throw new InvalidOperationException("Unexpected response received during device link enrollment.");
            
            var mobileEcdhPublicKey = ProtocolSerializer.ParsePairingResponse(response);
            var sharedSecret = SecureChannelModel.DeriveSharedSecret(ecdhKeyPair, mobileEcdhPublicKey);

            var verificationCode = SecureChannelModel.ComputeVerificationCode(sharedSecret);
            var codeConfirmed = await ShowVerificationCodeAsync(verificationCode);
            if (!codeConfirmed)
                throw new UnauthorizedAccessException("Device link pairing was rejected by the user.");

            var pairingId = Guid.NewGuid().ToString();
            var credentialId = Guid.NewGuid().ToString();
            
            var confirmationMessage = ProtocolSerializer.CreatePairingConfirmMessage(credentialId, VaultName, pairingId);
            await connectedDevice.SendMessageAsync(confirmationMessage, cancellationToken);
            var completeResponse = await connectedDevice.ReceiveMessageAsync(cancellationToken);
            
            messageType = (MessageType)completeResponse[0];
            if (messageType != MessageType.PairingComplete)
                throw new InvalidOperationException("Unexpected response received during device link enrollment.");
            
            var signingPublicKey = ProtocolSerializer.ParsePairingComplete(completeResponse);
            return new DeviceLinkPairingResult(ManagedKey.TakeOwnership(sharedSecret))
            {
                CredentialId = credentialId,
                PairingId = pairingId,
                MobileDeviceId = discoveredDevice.DeviceId,
                MobileDeviceName = discoveredDevice.DeviceName,
                PublicSigningKey = signingPublicKey
            };
        }

        /// <inheritdoc/>
        public override Task<IResult<IKeyBytes>> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Displays the verification code to the user for validation or acknowledgment.
        /// </summary>
        /// <param name="code">The verification code to be displayed.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is <c>true</c> if the code was confirmed; otherwise, if rejected, <c>false</c>.</returns>
        protected abstract Task<bool> ShowVerificationCodeAsync(string code);
    }
}
