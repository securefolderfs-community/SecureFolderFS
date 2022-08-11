using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsSettingsService"/>
    internal sealed class VaultsSettingsService : MultipleFilesSettingsModel, IVaultsSettingsService
    {
        public VaultsSettingsService(IModifiableFolder? settingsFolder)
        {
            SettingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        protected override string? SettingsStorageName { get; } = Constants.LocalSettings.VAULTS_SETTINGS_FOLDERNAME;

        /// <inheritdoc/>
        public VaultContextDataModel GetVaultContextForId(string id)
        {
            var vaultContext = GetSetting<VaultContextDataModel>(() => new(), id);
            return vaultContext!; // TODO: Ensure not null
        }
    }
}
