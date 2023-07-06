using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.UI.ServiceImplementation.VaultPersistence;
using System.Threading;
using System.Threading.Tasks;

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
        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(VaultWidgets.LoadAsync(cancellationToken), VaultConfigurations.LoadAsync(cancellationToken));
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(VaultWidgets.SaveAsync(cancellationToken), VaultConfigurations.SaveAsync(cancellationToken));
        }
    }
}
