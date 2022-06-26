using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    public sealed class LocalVaultCollectionModel : IVaultCollectionModel
    {
        private bool _vaultsLoaded;
        private readonly List<IVaultModel> _vaults;

        private IVaultsSettingsService VaultsSettingsService { get; } = Ioc.Default.GetRequiredService<IVaultsSettingsService>();

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

            if (vault is not VaultModel vaultModel)
                return false;

            VaultsSettingsService.VaultPaths ??= new();
            VaultsSettingsService.VaultPaths.Add(vaultModel.Folder.Path);

            await VaultsSettingsService.SaveSettingsAsync(cancellationToken);

            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || VaultsSettingsService.VaultPaths is null)
                return false;

            var indexToRemove = VaultsSettingsService.VaultPaths.FindIndex(x => vault.Folder.Path.Equals(x));
            if (indexToRemove == -1)
                return false;

            VaultsSettingsService.VaultPaths.RemoveAt(indexToRemove);
            await VaultsSettingsService.SaveSettingsAsync(cancellationToken);

            return true;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IVaultModel> GetVaultsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || VaultsSettingsService.VaultPaths is null)
                yield break;

            if (VaultsSettingsService.VaultPaths.IsEmpty())
                yield break;

            if (!_vaults.IsEmpty())
            {
                foreach (var item in _vaults)
                {
                    yield return item;
                }
            }

            foreach (var item in VaultsSettingsService.VaultPaths)
            {
                var folder = await FileSystemService.GetFolderFromPathAsync(item);
                if (folder is not null)
                {
                    var vaultModel = new VaultModel(folder);
                    _vaults.Add(vaultModel);

                    yield return vaultModel;
                }
            }
        }

        private async Task<bool> EnsureSettingsLoaded(CancellationToken cancellationToken)
        {
            _vaultsLoaded |= !_vaultsLoaded && await VaultsSettingsService.LoadSettingsAsync(cancellationToken);
            return _vaultsLoaded;
        }
    }
}
