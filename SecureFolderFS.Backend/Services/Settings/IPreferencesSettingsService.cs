using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Backend.Services.Settings
{
    public interface IPreferencesSettingsService : IBaseSettingsService
    {
        FileSystemAdapterType ActiveFileSystemAdapter { get; set; }

        bool StartOnSystemStartup { get; set; }

        bool ContinueOnLastVault { get; set; }

        bool AutoOpenVaultFolder { get; set; }
    }
}
