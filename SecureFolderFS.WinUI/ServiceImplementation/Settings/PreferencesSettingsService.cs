using SecureFolderFS.Backend.Services.Settings;
using SecureFolderFS.WinUI.Serialization;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    internal sealed class PreferencesSettingsService : BaseJsonSettings, IPreferencesSettingsService
    {
        public PreferencesSettingsService(ISettingsSharingContext settingsSharingContext)
        {
            RegisterSettingsContext(settingsSharingContext);
        }

        public bool StartOnSystemStartup
        {
            get => Get(() => false);
            set => Set(value);
        }

        public bool ContinueOnLastVault
        {
            get => Get(() => false);
            set => Set(value);
        }

        public bool AutoOpenVaultFolder 
        { 
            get => Get(() => false); 
            set => Set(value);
        }
    }
}
