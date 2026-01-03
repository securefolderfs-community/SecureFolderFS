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
        private VaultKeystoreDataModel? _keystoreDataModel;
        private VaultConfigurationDataModel? _configDataModel;
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
            _keystoreDataModel = passkey.UseKey(key => VaultParser.EncryptKeystore(key, dekKey, macKey, salt));

            // Create key copies for later use
            _dekKey = SecureKey.TakeOwnership(dekKey);
            _macKey = SecureKey.TakeOwnership(macKey);
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            _configDataModel = VaultConfigurationDataModel.FromVaultOptions(vaultOptions);
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
                VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // Write the whole configuration
            await _vaultWriter.WriteKeystoreAsync(_keystoreDataModel, cancellationToken);
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
