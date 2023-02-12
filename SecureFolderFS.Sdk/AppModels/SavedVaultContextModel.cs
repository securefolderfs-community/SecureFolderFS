using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="IVaultContextModel"/>
    public sealed class SavedVaultContextModel : BaseSerializedDataModel<IVaultConfiguration>, IVaultContextModel
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

            var vaultContext = SettingsService.GetVaultContextForId(VaultModel.Folder.Id);

            return vaultContext.LastAccessedDate;
        }

        /// <inheritdoc/>
        public async Task<bool> SetLastAccessedDate(DateTime? value, CancellationToken cancellationToken = default)
        {
            if (!await EnsureSettingsLoaded(cancellationToken))
                return false;

            var vaultContext = SettingsService.GetVaultContextForId(VaultModel.Folder.Id);
            vaultContext.LastAccessedDate = value;

            return await SettingsService.SaveAsync(cancellationToken);
        }
    }
}
