using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.Models;
using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;
using static SecureFolderFS.Core.Constants.Vault;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <inheritdoc cref="ICreationRoutine"/>
    internal sealed class CreationRoutine : ICreationRoutine
    {
        private readonly IFolder _vaultFolder;
        private readonly VaultWriter _vaultWriter;
        private V3VaultKeystoreDataModel? _keystoreDataModel;
        private V4VaultKeystoreDataModel? _v4KeystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
        private V4VaultConfigurationDataModel? _v4ConfigDataModel;
        private IKeyUsage? _dekKey;
        private IKeyUsage? _macKey;

        public CreationRoutine(IFolder vaultFolder, VaultWriter vaultWriter)
        {
            _vaultFolder = vaultFolder;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            // Allocate keys for later use
            var dekKey = new byte[KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[KeyTraits.MAC_KEY_LENGTH];
            var salt = new byte[KeyTraits.SALT_LENGTH];

            // Fill keys
            RandomNumberGenerator.Fill(dekKey);
            RandomNumberGenerator.Fill(macKey);
            RandomNumberGenerator.Fill(salt);

            // Generate keystore
            _keystoreDataModel = passkey.UseKey(key => VaultParser.V3EncryptKeystore(key, dekKey, macKey, salt));

            // Create key copies for later use
            _dekKey = SecureKey.TakeOwnership(dekKey);
            _macKey = SecureKey.TakeOwnership(macKey);
        }

        public void V4SetCredentials(IKeyUsage passkey)
        {
            // Allocate keys for later use
            var dekKey = new byte[KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[KeyTraits.MAC_KEY_LENGTH];
            var salt = new byte[KeyTraits.SALT_LENGTH];

            // Fill keys and salt
            RandomNumberGenerator.Fill(dekKey);
            RandomNumberGenerator.Fill(macKey);
            RandomNumberGenerator.Fill(salt);

            // Generate V4 keystore — SoftwareEntropy is generated internally by V4EncryptKeystore
            _v4KeystoreDataModel = passkey.UseKey(key => VaultParser.V4EncryptKeystore(key, dekKey, macKey, salt));

            // Create key copies for later use
            _dekKey = SecureKey.TakeOwnership(dekKey);
            _macKey = SecureKey.TakeOwnership(macKey);
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            if (vaultOptions.AppPlatform is null)
            {
                _configDataModel = VaultConfigurationDataModel.FromVaultOptions(vaultOptions);
                _v4ConfigDataModel = null;
            }
            else
            {
                _v4ConfigDataModel = V4VaultConfigurationDataModel.V4FromVaultOptions(vaultOptions);
                _configDataModel = _v4ConfigDataModel.ToVaultConfigurationDataModel();
            }
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_keystoreDataModel);
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_macKey);
            ArgumentNullException.ThrowIfNull(_dekKey);

            // First, we need to fill in the PayloadMac of the content
            _macKey.UseKey(macKey =>
            {
                if (_v4ConfigDataModel is not null)
                    VaultParser.V4CalculateConfigMac(_v4ConfigDataModel, macKey, _v4ConfigDataModel.PayloadMac);
                else
                    VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // Write the whole configuration
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
            //await _vaultWriter.WriteV4KeystoreAsync(_v4KeystoreDataModel, cancellationToken);
            if (_v4ConfigDataModel is not null)
                await _vaultWriter.WriteV4ConfigurationAsync(_v4ConfigDataModel, cancellationToken);
            else
                await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // Create the content folder
            if (_vaultFolder is IModifiableFolder modifiableFolder)
                await modifiableFolder.CreateFolderAsync(Names.VAULT_CONTENT_FOLDERNAME, true, cancellationToken);

            // Key copies need to be created because the original ones are disposed of here
            return new SecurityWrapper(KeyPair.ImportKeys(_dekKey, _macKey), _configDataModel);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _dekKey?.Dispose();
            _macKey?.Dispose();
        }
    }
}
