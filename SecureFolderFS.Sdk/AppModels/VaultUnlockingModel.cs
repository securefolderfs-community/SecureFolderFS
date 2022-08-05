using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a model used for unlocking vaults.
    /// </summary>
    public sealed class VaultUnlockingModel : IUnlockingModel<IUnlockedVaultModel>
    // TODO: This is bad. IUnlockingModel is too simple to proxy routine for unlocking vaults. Create a new model that produces more detailed results (similar to IVaultCreationModel).
    {
        private readonly IVaultModel _vaultModel;
        private readonly IKeystoreModel _keystoreModel;

        private IVaultUnlockingService VaultUnlockingService { get; } = Ioc.Default.GetRequiredService<IVaultUnlockingService>();

        public VaultUnlockingModel(IVaultModel vaultModel, IKeystoreModel keystoreModel)
        {
            _vaultModel = vaultModel;
            _keystoreModel = keystoreModel;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Maybe set the lock there?

            // Set the vault folder
            await VaultUnlockingService.SetVaultFolderAsync(_vaultModel.Folder, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IUnlockedVaultModel?> UnlockAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            // Try set the lock
            _ = await _vaultModel.LockFolderAsync();

            using (_vaultModel.FolderLock)
            using (password)
            {
                // Get the keystore
                await using var keystoreStream = await _keystoreModel.GetKeystoreStreamAsync(cancellationToken);
                if (keystoreStream is null)
                    return null; // TODO: Report issue

                var configFile = await _vaultModel.Folder.GetFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, cancellationToken);
                if (configFile is null)
                    return null;

                // Set the config file
                if (!await VaultUnlockingService.SetConfigurationAsync(configFile, cancellationToken))
                    return null;

                // Set the keystore stream
                if (!await VaultUnlockingService.SetKeystoreStreamAsync(keystoreStream, JsonToStreamSerializer.Instance, cancellationToken))
                    return null; // TODO: Report the issue

                // Unlock the vault
                return await VaultUnlockingService.UnlockAndStartAsync(password, cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultUnlockingService.Dispose();
            _keystoreModel.Dispose();
        }
    }
}
