using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
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
    public sealed class LocalVaultCollectionModel : BaseSerializedDataModel<ISavedVaultsService>, IVaultCollectionModel
    {
        private List<IVaultModel>? _vaults;

        private IStorageService StorageService { get; } = Ioc.Default.GetRequiredService<IStorageService>();

        /// <inheritdoc/>
        public async Task<bool> HasVaultsAsync(CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            return !SettingsService.VaultPaths.IsEmpty();
        }

        /// <inheritdoc/>
        public async Task<bool> AddVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            if (vault.Folder is not ILocatableFolder vaultFolder)
                return false;

            _vaults?.Add(vault);

            SettingsService.VaultPaths ??= new();
            SettingsService.VaultPaths.Add(vaultFolder.Path);

            return await SettingsService.SaveSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || SettingsService.VaultPaths is null)
                return false;

            if (vault.Folder is not ILocatableFolder vaultFolder)
                return false;

            var indexToRemove = SettingsService.VaultPaths.FindIndex(x => vaultFolder.Path.Equals(x));
            if (indexToRemove == -1)
                return false;

            _vaults?.Remove(vault);
            SettingsService.VaultPaths.RemoveAt(indexToRemove);

            return await SettingsService.SaveSettingsAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IVaultModel> GetVaultsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || SettingsService.VaultPaths is null)
                yield break;

            if (SettingsService.VaultPaths.IsEmpty())
                yield break;

            if (_vaults is not null && !_vaults.IsEmpty())
            {
                foreach (var item in _vaults)
                {
                    yield return item;
                }
            }

            _vaults ??= new();
            foreach (var item in SettingsService.VaultPaths)
            {
                var folder = await StorageService.TryGetFolderFromPathAsync(item, cancellationToken);
                if (folder is not null)
                {
                    var vaultModel = new LocalVaultModel(folder);
                    _vaults.Add(vaultModel);

                    yield return vaultModel;
                }
            }
        }
    }
}
