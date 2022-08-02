using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IPreferencesSettingsService"/>
    internal sealed class PreferencesSettingsService : SharedSettingsModel, IPreferencesSettingsService
    {
        public PreferencesSettingsService(ISettingsDatabaseModel originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsModel)
        {
            SettingsDatabase = originSettingsDatabase;
        }

        /// <inheritdoc/>
        public string PreferredFileSystemId
        {
            get => GetSetting(() => string.Empty);
            set => SetSetting(value);
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
            get => GetSetting(() => false);
            set => SetSetting(value);
        }
    }
}
