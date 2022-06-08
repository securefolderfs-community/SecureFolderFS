using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.WinUI.Serialization;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    internal sealed class GeneralSettingsService : BaseJsonSettings, IGeneralSettingsService
    {
        public GeneralSettingsService(ISettingsSharingContext settingsSharingContext)
        {
            RegisterSettingsContext(settingsSharingContext);
        }
    }
}
