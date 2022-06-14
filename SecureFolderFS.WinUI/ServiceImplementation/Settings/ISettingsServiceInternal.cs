using SecureFolderFS.WinUI.Serialization;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    internal interface ISettingsServiceInternal
    {
        ISettingsSharingContext GetSharingContext();
    }
}
