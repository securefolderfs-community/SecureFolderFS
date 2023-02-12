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
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    public sealed class LocalVaultCollectionModel : IVaultCollectionModel
    {
        private List<IVaultModel>? _vaults;

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        private IVaultConfigurations VaultConfigurations { get; } = Ioc.Default.GetRequiredService<IVaultPersistenceService>().VaultConfigurations;

        /// <inheritdoc/>
        public bool IsEmpty => VaultConfigurations.SavedVaults.IsEmpty();

        /// <inheritdoc/>
        public async Task<bool> AddVaultAsync(IVaultModel vaultModel, CancellationToken cancellationToken = default)
        {
            _vaults?.Add(vaultModel);

            VaultConfigurations.SavedVaults ??= new List<VaultDataModel>();
            VaultConfigurations.SavedVaults.Add(new()
            {
                Id = vaultModel.Folder.Id,
                Name = vaultModel.Folder.Name,
                LastAccessDate = vaultModel.LastAccessDate
            });

            return await VaultConfigurations.SaveAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (VaultConfigurations.SavedVaults is null)
                return false;

            if (vault.Folder is not ILocatableFolder vaultFolder)
                return false;

            var indexToRemove = VaultConfigurations.SavedVaults.FindIndex(x => vaultFolder.Path == x.Id);
            if (indexToRemove == -1)
                return false;

            _vaults?.Remove(vault);
            VaultConfigurations.SavedVaults.RemoveAt(indexToRemove);

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

                var vaultModel = new LocalVaultModel(folder, item.Name, item.LastAccessDate);
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
