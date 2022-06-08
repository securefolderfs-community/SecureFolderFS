using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.WinUI.Serialization;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    internal sealed class SecuritySettingsService : BaseJsonSettings, ISecuritySettingsService
    {
        public SecuritySettingsService(ISettingsSharingContext settingsSharingContext)
        {
            RegisterSettingsContext(settingsSharingContext);
        }

        public bool EnableAuthentication
        {
            get => Get(() => false);
            set => Set(value);
        }

        public bool AutomaticallyLockVaults
        {
            get => Get(() => false);
            set => Set(value);
        }
    }
}
