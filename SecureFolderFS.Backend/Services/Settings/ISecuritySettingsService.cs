namespace SecureFolderFS.Backend.Services.Settings
{
    public interface ISecuritySettingsService : IBaseSettingsService
    {
        bool EnableAuthentication { get; set; }

        bool AutomaticallyLockVaults { get; set; }
    }
}
