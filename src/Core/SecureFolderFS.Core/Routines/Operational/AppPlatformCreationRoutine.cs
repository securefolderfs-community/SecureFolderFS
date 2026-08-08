using System;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;
using static SecureFolderFS.Core.Constants.Vault;
using static SecureFolderFS.Core.Cryptography.Constants;

namespace SecureFolderFS.Core.Routines.Operational
{
    /// <summary>
    /// Creation routine for App Platform vaults. Generates DEK+MAC internally (no password, no keystore.cfg).
    /// </summary>
    public sealed class AppPlatformCreationRoutine : ICreationRoutine
    {
        private readonly IFolder _vaultFolder;
        private readonly VaultWriter _vaultWriter;
        private VaultConfigurationDataModel? _configDataModel;
        private SecureKey? _dekKey;
        private SecureKey? _macKey;

        public AppPlatformCreationRoutine(IFolder vaultFolder, VaultWriter vaultWriter)
        {
            _vaultFolder = vaultFolder;
            _vaultWriter = vaultWriter;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            var dekKey = new byte[KeyTraits.DEK_KEY_LENGTH];
            var macKey = new byte[KeyTraits.MAC_KEY_LENGTH];

            RandomNumberGenerator.Fill(dekKey);
            RandomNumberGenerator.Fill(macKey);

            _dekKey = SecureKey.TakeOwnership(dekKey);
            _macKey = SecureKey.TakeOwnership(macKey);

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void SetCredentials(IKeyUsage passkey)
        {
            // No-op: App Platform vaults don't use passkey-derived keys
        }

        /// <inheritdoc/>
        public void SetOptions(VaultOptions vaultOptions)
        {
            _configDataModel = VaultConfigurationDataModel.V4FromVaultOptions(vaultOptions);
        }

        /// <inheritdoc/>
        public async Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_configDataModel);
            ArgumentNullException.ThrowIfNull(_dekKey);
            ArgumentNullException.ThrowIfNull(_macKey);

            _macKey.UseKey(macKey =>
            {
                VaultParser.CalculateConfigMac(_configDataModel, macKey, _configDataModel.PayloadMac);
            });

            // Write only sfconfig.cfg - no keystore.cfg for App Platform vaults
            await _vaultWriter.WriteConfigurationAsync(_configDataModel, cancellationToken);

            // Create the content folder
            if (_vaultFolder is IModifiableFolder modifiableFolder)
                await modifiableFolder.CreateFolderAsync(Names.VAULT_CONTENT_FOLDERNAME, true, cancellationToken);

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
