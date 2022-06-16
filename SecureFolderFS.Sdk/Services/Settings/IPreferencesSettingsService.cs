using SecureFolderFS.Core.Enums;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.Settings
{
    public interface IPreferencesSettingsService : ISettingsModel
    {
        FileSystemAdapterType ActiveFileSystemAdapter { get; set; }

        bool StartOnSystemStartup { get; set; }

        bool ContinueOnLastVault { get; set; }

        bool AutoOpenVaultFolder { get; set; }
    }
}
