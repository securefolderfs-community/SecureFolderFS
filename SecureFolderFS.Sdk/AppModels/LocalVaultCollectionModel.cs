using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    public sealed class LocalVaultCollectionModel : BaseSerializedDataModel<ISavedVaultsService>, IVaultCollectionModel
    {
        private readonly List<IVaultModel> _vaults;

        private IFileSystemService FileSystemService { get; } = Ioc.Default.GetRequiredService<IFileSystemService>();

        public LocalVaultCollectionModel()
        {
            _vaults = new();
        }

        /// <inheritdoc/>
        public async Task<bool> AddVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            if (vault.Folder is not ILocatableFolder vaultFolder)
                return false;

            SettingsService.VaultPaths ??= new();
            SettingsService.VaultPaths.Add(vaultFolder.Path);

            await SettingsService.SaveSettingsAsync(cancellationToken);

            return true;
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

            SettingsService.VaultPaths.RemoveAt(indexToRemove);
            await SettingsService.SaveSettingsAsync(cancellationToken);

            return true;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IVaultModel> GetVaultsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || SettingsService.VaultPaths is null)
                yield break;

            if (SettingsService.VaultPaths.IsEmpty())
                yield break;

            if (!_vaults.IsEmpty())
            {
                foreach (var item in _vaults)
                {
                    yield return item;
                }
            }

            foreach (var item in SettingsService.VaultPaths)
            {
                var folder = await FileSystemService.GetFolderFromPathAsync(item);
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
