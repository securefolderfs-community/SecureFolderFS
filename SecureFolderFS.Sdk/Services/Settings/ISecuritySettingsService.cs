using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.Settings
{
    public interface ISecuritySettingsService : ISettingsModel
    {
        bool EnableAuthentication { get; set; }

        bool AutomaticallyLockVaults { get; set; }
    }
}
