using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IPreferencesSettingsService"/>
    internal sealed class PreferencesSettingsService : SharedSettingsModel, IPreferencesSettingsService
    {
        public PreferencesSettingsService(IDatabaseModel<string> originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsDatabase, originSettingsModel)
        {
        }

        /// <inheritdoc/>
        public string? PreferredFileSystemId
        {
            get => GetSetting<string?>(() => Core.Constants.FileSystemId.WEBDAV_ID);
            set => SetSetting<string?>(value);
        }

        /// <inheritdoc/>
        public bool StartOnSystemStartup
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool ContinueOnLastVault
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool OpenFolderOnUnlock
        {
            get => GetSetting(() => true);
            set => SetSetting(value);
        }
    }
}
