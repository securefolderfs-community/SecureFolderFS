using System;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.AppModels.Database;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IAppSettings"/>
    public sealed class AppSettings : SettingsModel, IAppSettings
    {
        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public AppSettings(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.APPLICATION_SETTINGS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
        }

        /// <inheritdoc/>
        public bool WasBetaNotificationShown1
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool WasVaultFolderExplanationShown
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
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
            get => GetSetting<string?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? ApplicationTheme
        {
            get => GetSetting<string?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? LastVersion
        {
            get => GetSetting<string?>();
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public string? AppLanguage
        {
            get => GetSetting<string?>();
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
