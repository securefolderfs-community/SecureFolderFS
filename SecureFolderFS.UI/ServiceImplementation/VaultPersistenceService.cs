using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.ServiceImplementation.VaultPersistence;

namespace SecureFolderFS.UI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultPersistenceService"/>
    public sealed class VaultPersistenceService : IVaultPersistenceService
    {
        /// <inheritdoc/>
        public IVaultWidgets VaultWidgets { get; }

        /// <inheritdoc/>
        public IVaultConfigurations VaultConfigurations { get; }

        public VaultPersistenceService(IModifiableFolder settingsFolder)
        {
            VaultWidgets = new VaultsWidgets(settingsFolder);
            VaultConfigurations = new VaultConfigurations(settingsFolder);
        }

        /// <inheritdoc/>
        public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            var result = true;
            result &= await VaultWidgets.LoadAsync(cancellationToken);
            result &= await VaultConfigurations.LoadAsync(cancellationToken);

            return result;
        }

        /// <inheritdoc/>
        public async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            var result = true;
            result &= await VaultWidgets.SaveAsync(cancellationToken);
            result &= await VaultConfigurations.SaveAsync(cancellationToken);

            return result;
        }
    }
}
