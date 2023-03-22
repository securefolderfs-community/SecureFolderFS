using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.VaultPersistence;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Extensions;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    public sealed class LocalVaultCollectionModel : IVaultCollectionModel
    {
        private List<IVaultModel>? _vaults;

        private IVaultWidgets VaultWidgets { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultWidgets;

        private IVaultConfigurations VaultConfigurations { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultConfigurations;

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        /// <inheritdoc/>
        public bool IsEmpty => VaultConfigurations.SavedVaults.IsEmpty();

        /// <inheritdoc/>
        public async Task<bool> AddVaultAsync(IVaultModel vaultModel, CancellationToken cancellationToken = default)
        {
            _vaults?.Add(vaultModel);

            VaultConfigurations.SavedVaults ??= new List<VaultDataModel>();
            VaultConfigurations.SavedVaults.Add(new(vaultModel.Folder.Id, vaultModel.VaultName, vaultModel.LastAccessDate));
            VaultWidgets.SetForVault(vaultModel.Folder.Id, new List<WidgetDataModel>()
            {
                new(Constants.Widgets.HEALTH_WIDGET_ID),
                new(Constants.Widgets.GRAPHS_WIDGET_ID)
            });

            var results = await Task.WhenAll(VaultWidgets.SaveAsync(cancellationToken), VaultConfigurations.SaveAsync(cancellationToken));
            return results[0] && results[1];
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveVaultAsync(IVaultModel vaultModel, CancellationToken cancellationToken = default)
        {
            if (VaultConfigurations.SavedVaults is null)
                return false;

            if (vaultModel.Folder is not ILocatableFolder vaultFolder)
                return false;

            var itemToRemove = VaultConfigurations.SavedVaults.FirstOrDefault(x => vaultFolder.Id == x.Id);
            if (itemToRemove is null)
                return false;

            _vaults?.Remove(vaultModel);
            VaultConfigurations.SavedVaults.Remove(itemToRemove);

            return await VaultConfigurations.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IVaultModel> GetVaultsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (IsEmpty || VaultConfigurations.SavedVaults is null)
                yield break;

            if (_vaults is not null && !_vaults.IsEmpty())
            {
                foreach (var item in _vaults)
                    yield return item;
            }

            _vaults ??= new();
            foreach (var item in VaultConfigurations.SavedVaults)
            {
                if (string.IsNullOrEmpty(item.Id))
                    continue;

                var folder = await StorageService.TryGetFolderFromPathAsync(item.Id, cancellationToken);
                if (folder is null)
                    continue;

                var vaultModel = new LocalVaultModel(folder, item.VaultName, item.LastAccessDate);
                _vaults.Add(vaultModel);

                yield return vaultModel;
            }
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await VaultConfigurations.LoadAsync(cancellationToken);
        }
    }
}
