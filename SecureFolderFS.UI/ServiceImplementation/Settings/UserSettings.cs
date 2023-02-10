using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System.ComponentModel;

namespace SecureFolderFS.UI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IUserSettings"/>
    public sealed class UserSettings : LocalSettingsModel, IUserSettings
    {
        public UserSettings(IModifiableFolder settingsFolder)
            : base(settingsFolder)
        {
        }

        #region Privacy Settings

        /// <inheritdoc/>
        public bool AutoLockVaults
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool IsTelemetryEnabled
        {
            get => GetSetting(() => false); // TODO: Enable as default
            set => SetSetting(value);
        }

        #endregion

        #region Preferences

        /// <inheritdoc/>
        public string PreferredFileSystemId
        {
            get => GetSetting(static () => Core.Constants.FileSystemId.WEBDAV_ID);
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

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
