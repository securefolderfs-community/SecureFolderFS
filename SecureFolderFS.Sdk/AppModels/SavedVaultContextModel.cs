using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

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

            if (VaultModel.Folder is not ILocatableFolder vaultFolder)
                return null;

            var vaultContext = SettingsService.GetVaultContextForId(vaultFolder.Path);
            return vaultContext.LastAccessedDate;
        }

        /// <inheritdoc/>
        public async Task<bool> SetLastAccessedDate(DateTime? value, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            if (VaultModel.Folder is not ILocatableFolder vaultFolder)
                return false;

            var vaultContext = SettingsService.GetVaultContextForId(vaultFolder.Path);
            vaultContext.LastAccessedDate = value;

            return await SettingsService.SaveSettingsAsync(cancellationToken);
        }
    }
}
