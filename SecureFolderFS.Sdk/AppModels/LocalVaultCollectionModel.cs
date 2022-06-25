using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultCollectionModel"/>
    public sealed class LocalVaultCollectionModel : IVaultCollectionModel
    {
        private bool _vaultsLoaded;

        private IVaultsSettingsService VaultsSettingsService { get; } = Ioc.Default.GetRequiredService<IVaultsSettingsService>();

        /// <inheritdoc/>
        public async Task<bool> AddVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            if (vault is not VaultModel vaultModel)
                return false;

            VaultsSettingsService.SavedVaults ??= new();
            VaultsSettingsService.SavedVaults.Add(vaultModel);
            return true;
        }

        /// <inheritdoc/>
        public async Task<bool> RemoveVaultAsync(IVaultModel vault, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || VaultsSettingsService.SavedVaults is null)
                return false;

            var indexToRemove = VaultsSettingsService.SavedVaults.FindIndex(x => vault.Equals(x));
            if (indexToRemove == -1)
                return false;

            VaultsSettingsService.SavedVaults.RemoveAt(indexToRemove);
            return true;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IVaultModel> GetVaultsAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken) || VaultsSettingsService.SavedVaults is null)
                yield break;

            foreach (var item in VaultsSettingsService.SavedVaults)
            {
                yield return item;
            }
        }

        private async Task<bool> EnsureSettingsLoaded(CancellationToken cancellationToken)
        {
            _vaultsLoaded &= !_vaultsLoaded && await VaultsSettingsService.LoadSettingsAsync(cancellationToken);
            return _vaultsLoaded;
        }
    }
}
