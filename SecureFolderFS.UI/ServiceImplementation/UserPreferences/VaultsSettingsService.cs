using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IVaultsSettingsService"/>
    public sealed class VaultsSettingsService : SettingsModel, IVaultsSettingsService
    {
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public VaultsSettingsService(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.LocalSettings.SAVED_VAULTS_FILENAME, settingsFolder, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public VaultDataModel GetVaultDataForId(string id)
        {
            return GetSetting<VaultDataModel>(() => new(), id);
        }
    }
}
