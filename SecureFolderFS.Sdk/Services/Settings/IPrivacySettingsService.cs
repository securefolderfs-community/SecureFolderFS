using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// A service to manage privacy related settings.
    /// </summary>
    public interface IPrivacySettingsService : ISettingsModel
    {
        /// <summary>
        /// Determines whether to lock all unlocked vaults when the system is locked.
        /// </summary>
        bool VaultAutoLock { get; set; }
    }
}
