using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.AppModels.Database;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System.ComponentModel;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public sealed class UserSettings : SettingsModel, IUserSettings
    {
        private ITelemetryService TelemetryService { get; } = Ioc.Default.GetRequiredService<ITelemetryService>();

        /// <inheritdoc/>
        protected override IDatabaseModel<string> SettingsDatabase { get; }

        public UserSettings(IModifiableFolder settingsFolder)
        {
            SettingsDatabase = new SingleFileDatabaseModel(Constants.FileNames.USER_SETTINGS_FILENAME, settingsFolder, DoubleSerializedStreamSerializer.Instance);
            PropertyChanged += UserSettings_PropertyChanged;
        }

        #region Preferences

        /// <inheritdoc/>
        public string PreferredFileSystemId
        {
            get => GetSetting(static () => Core.Constants.FileSystemId.FS_WEBDAV);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool StartOnSystemStartup
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool ContinueOnLastVault
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool OpenFolderOnUnlock
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        #endregion

        #region Privacy

        /// <inheritdoc/>
        public bool AutoLockVaults
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool IsTelemetryEnabled
        {
            get => GetSetting(() => true);
            set => SetSetting(value);
        }

        #endregion

        private void UserSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            var eventName = e.PropertyName switch
            {
                nameof(AutoLockVaults) => $"{nameof(AutoLockVaults)}: {AutoLockVaults}",
                nameof(IsTelemetryEnabled) => $"{nameof(IsTelemetryEnabled)}: {IsTelemetryEnabled}",
                nameof(PreferredFileSystemId) => $"{nameof(PreferredFileSystemId)}: {PreferredFileSystemId}",
                nameof(StartOnSystemStartup) => $"{nameof(StartOnSystemStartup)}: {StartOnSystemStartup}",
                nameof(ContinueOnLastVault) => $"{nameof(ContinueOnLastVault)}: {ContinueOnLastVault}",
                nameof(OpenFolderOnUnlock) => $"{nameof(OpenFolderOnUnlock)}: {OpenFolderOnUnlock}",
                _ => null
            };

            if (eventName is not null)
                TelemetryService.TrackEvent(eventName);
        }
    }
}
