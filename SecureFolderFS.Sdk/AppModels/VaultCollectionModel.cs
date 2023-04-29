using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    public sealed class VaultCollectionModel : IVaultCollectionModel
    {
        private readonly List<IVaultModel> _vaults;

        private IVaultWidgets VaultWidgets { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultWidgets;

        private IVaultConfigurations VaultConfigurations { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultConfigurations;

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        public VaultCollectionModel()
        {
            _vaults = new();
        }

        /// <inheritdoc/>
        public bool AddVault(IVaultModel vaultModel)
        {
            // Add to cache
            _vaults.Add(vaultModel);

            // Update saved vaults
            VaultConfigurations.SavedVaults ??= new List<VaultDataModel>();
            VaultConfigurations.SavedVaults.Add(new(vaultModel.Folder.Id, vaultModel.VaultName, vaultModel.LastAccessDate));

            // Update widgets
            VaultWidgets.SetForVault(vaultModel.Folder.Id, new List<WidgetDataModel>()
            {
                new(Constants.Widgets.HEALTH_WIDGET_ID),
                new(Constants.Widgets.GRAPHS_WIDGET_ID)
            });

            return true;
        }

        /// <inheritdoc/>
        public bool RemoveVault(IVaultModel vaultModel)
        {
            var itemToRemove = VaultConfigurations.SavedVaults?.FirstOrDefault(x => vaultModel.Folder.Id == x.Id);
            if (itemToRemove is null)
                return false;

            // Remove from cache
            _vaults.Remove(vaultModel);

            // Remove persisted
            VaultConfigurations.SavedVaults!.Remove(itemToRemove);
            VaultWidgets.SetForVault(vaultModel.Folder.Id, null);

            return true;
        }

        /// <inheritdoc/>
        public IEnumerable<IVaultModel> GetVaults()
        {
            return _vaults;
        }

        /// <inheritdoc/>
        public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            var result = true;
            result &= await VaultConfigurations.LoadAsync(cancellationToken);
            result &= await VaultWidgets.LoadAsync(cancellationToken);

            // Clear previous vaults
            _vaults.Clear();

            VaultConfigurations.SavedVaults ??= new List<VaultDataModel>();
            foreach (var item in VaultConfigurations.SavedVaults)
            {
                if (item.Id is null)
                    continue;

                var folder = await StorageService.TryGetFolderFromPathAsync(item.Id, cancellationToken);
                if (folder is null)
                    continue;

                var vaultModel = new VaultModel(folder, item.VaultName, item.LastAccessDate);
                _vaults.Add(vaultModel);
            }

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
