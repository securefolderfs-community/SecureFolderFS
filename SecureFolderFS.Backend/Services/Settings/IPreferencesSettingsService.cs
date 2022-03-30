namespace SecureFolderFS.Backend.Services.Settings
{
    public interface IPreferencesSettingsService : IBaseSettingsService
    {
        bool StartOnSystemStartup { get; set; }

        bool ContinueOnLastVault { get; set; }

        bool AutoOpenVaultFolder { get; set; }
    }
}
