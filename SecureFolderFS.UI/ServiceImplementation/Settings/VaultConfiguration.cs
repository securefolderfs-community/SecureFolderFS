using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IVaultConfiguration"/>
    public sealed class VaultsSettingsService : SettingsModel, IVaultConfiguration
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public VaultsSettingsService(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.LocalSettings.SAVED_VAULTS_FILENAME, settingsFolder, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public IList<VaultDataModel>? SavedVaults
        {
            get => GetSetting<IList<VaultDataModel>>();
            set => SetSetting(value);
        }
    }
}
