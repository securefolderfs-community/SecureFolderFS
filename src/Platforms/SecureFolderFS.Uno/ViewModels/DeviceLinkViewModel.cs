using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.PhoneLink.Enums;
using SecureFolderFS.Sdk.PhoneLink.Models;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.UI.DataModels;

namespace SecureFolderFS.Uno.ViewModels
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

        /// <inheritdoc/>
        public override bool CanComplement { get; } = true;

        /// <inheritdoc/>
        public override AuthenticationStage Availability { get; } = AuthenticationStage.Any;
        
        protected DeviceLinkViewModel(IFolder vaultFolder, string vaultId)
            : base(Core.Constants.Vault.Authentication.AUTH_DEVICE_LINK)
        {
            VaultFolder = vaultFolder;
            VaultId = vaultId;
        }
        
        /// <inheritdoc/>
        public override Task RevokeAsync(string? id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override Task<IKeyBytes> EnrollAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override async Task<IKeyBytes> AcquireAsync(string id, byte[]? data, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(data);
            
            // var discovery = new DeviceDiscovery();
            // var config = await GetDataModelAsync(cancellationToken);
            // while (!cancellationToken.IsCancellationRequested)
            // {
            //     var devices = await discovery.DiscoverDevicesAsync(cancellationToken: cancellationToken);
            //     var pairedDevice = devices.FirstOrDefault(d => d.DeviceId == config.EndpointDeviceId);
            //     if (pairedDevice is null)
            //     {
            //         await Task.Delay(1500, cancellationToken);
            //         continue;
            //     }
            //     
            //     var dataModel = await GetDataModelAsync(cancellationToken);
            //     var signed = await AuthenticateAsync(data, VaultId, pairedDevice, dataModel, cancellationToken);
            //     _ = signed ?? throw new InvalidOperationException("Authentication failed.");
            //
            //     return ManagedKey.TakeOwnership(signed);
            // }

            throw new OperationCanceledException();
        }

        public async Task<byte[]?> AuthenticateAsync(
            byte[] challenge,
            string vaultId,
            DiscoveredDevice device,
            DeviceLinkVaultDataModel dataModel,
            CancellationToken cancellationToken = default)
        {
            return null;
            // try
            // {
                // Step 2: Load shared secret
                // var sharedSecret = await _configStore.GetSharedSecretAsync(vaultId, config.CredentialId);
                // if (sharedSecret == null)
                //    return null;

                // // Step 3: Connect to the paired device
                // await ConnectToDeviceAsync(device, cancellationToken);
                //
                // // Step 4: Establish secure session
                // var sessionEstablished = await EstablishSecureSessionAsync(config, cancellationToken);
                // if (!sessionEstablished)
                //     return null;
                //
                // // Step 5: Send secure authentication request
                // var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                // var nonce = new byte[16];
                // RandomNumberGenerator.Fill(nonce);
                //
                // var authRequest = CreateSecureAuthRequest(dataModel.CredentialId, challenge, timestamp, nonce);
                // var encryptedRequest = _secureChannel!.Encrypt(authRequest);
                // await SendMessageAsync(encryptedRequest, MessageType.SecureAuthRequest, cancellationToken);
                //
                // // Step 6: Receive encrypted response
                // var encryptedResponse = await ReceiveMessageAsync(cancellationToken);
                // var messageType = (MessageType)encryptedResponse[0];
                //
                // if (messageType == MessageType.AuthenticationRejected)
                //     return null;
                //
                // if (messageType != MessageType.SecureAuthResponse)
                //     return null;
                //
                // // Decrypt response
                // var responsePayload = encryptedResponse.AsSpan(1).ToArray();
                // var decryptedResponse = _secureChannel.Decrypt(responsePayload);
                // var signature = decryptedResponse;
                //
                // // Step 7: Verify signature
                // using var verifyKey = ECDsa.Create();
                // verifyKey.ImportSubjectPublicKeyInfo(dataModel.PublicSigningKey, out _);
                //
                // // The mobile signs: CID + challenge + timestamp + nonce
                // var signedData = BuildSignedData(dataModel.CredentialId, challenge, timestamp, nonce);
                // var isValid = verifyKey.VerifyData(signedData, signature, HashAlgorithmName.SHA256);
                // if (!isValid)
                //     return null;
                //
                // // Step 8: Derive vault key using HKDF from the SHARED SECRET
                // // The shared secret is deterministic (established during pairing)
                // // The signature verification proves the mobile has the correct signing key
                // // We use CID as context to bind the key to this specific vault
                // var info = Encoding.UTF8.GetBytes($"PhoneLink-VaultKey-{dataModel.CredentialId}");
                // var salt = Encoding.UTF8.GetBytes(dataModel.PairingId ?? "");
                // var derivedKey = HKDF.DeriveKey(
                //     HashAlgorithmName.SHA256,
                //     sharedSecret, // Use shared secret, not signature
                //     32,
                //     salt,
                //     info);
                //
                // return derivedKey;
            // }
            // catch (Exception ex)
            // {
            //     return null;
            // }
            // finally
            // {
            //     Disconnect();
            // }
        }

        /// <summary>
        /// Asynchronously retrieves the data model representing the device link vault configuration.
        /// </summary>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> that cancels this action.</param>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. The value contains the <see cref="DeviceLinkVaultDataModel"/> instance.</returns>
        protected abstract Task<DeviceLinkVaultDataModel> GetDataModelAsync(CancellationToken cancellationToken = default);
    }
}
