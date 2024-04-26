using System.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.AppModels.Database;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public class UserSettings : SettingsModel, IUserSettings
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
        public virtual string PreferredFileSystemId
        {
            get => GetSetting(static () => Core.Constants.FileSystemId.FS_WEBDAV);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool StartOnSystemStartup
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool ContinueOnLastVault
        {
            get => GetSetting(static () => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool OpenFolderOnUnlock
        {
            get => GetSetting(static () => true);
            set => SetSetting(value);
        }

        #endregion

        #region Privacy

        /// <inheritdoc/>
        public virtual bool AutoLockVaults
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public virtual bool IsTelemetryEnabled
        {
            get => GetSetting(() => true);
            set => SetSetting(value);
        }

        #endregion

        protected virtual void UserSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
