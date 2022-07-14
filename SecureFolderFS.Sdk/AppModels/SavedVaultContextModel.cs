using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultContextModel"/>
    public sealed class SavedVaultContextModel : BaseSerializedDataModel<IVaultsSettingsService>, IVaultContextModel
    {
        /// <inheritdoc/>
        public IVaultModel VaultModel { get; }

        public SavedVaultContextModel(IVaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetLastAccessedDate(CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return null;

            if (SettingsService.VaultContexts is null || !SettingsService.VaultContexts.TryGetValue(VaultModel.Folder.Path, out var vaultContext))
                return null;

            return vaultContext.LastAccessedDate;
        }

        /// <inheritdoc/>
        public async Task<bool> SetLastAccessedDate(DateTime value, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            SettingsService.VaultContexts ??= new();

            if (SettingsService.VaultContexts.TryGetValue(VaultModel.Folder.Path, out var vaultContext))
                vaultContext.LastAccessedDate = value;
            else
            {
                SettingsService.VaultContexts[VaultModel.Folder.Path] = new(value);
            }

            return await SettingsService.SaveSettingsAsync(cancellationToken);
        }
    }
}
