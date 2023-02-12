using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;

namespace SecureFolderFS.UI.ServiceImplementation.SettingsPersistence
{
    /// <inheritdoc cref="IAppSettings"/>
    public sealed class AppSettings : SettingsModel, IAppSettings
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public AppSettings(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.LocalSettings.APPLICATION_SETTINGS_FILENAME, settingsFolder, StreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public bool IsIntroduced
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? LastVaultFolderId
        {
            get => GetSetting<string>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? ApplicationTheme
        {
            get => GetSetting<string>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public DateTime UpdateLastChecked
        {
            get => GetSetting<DateTime>(() => new());
            set => SetSetting(value);
        }
    }
}
