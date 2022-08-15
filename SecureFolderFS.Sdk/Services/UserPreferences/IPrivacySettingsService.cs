using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage privacy related settings.
    /// </summary>
    public interface IPrivacySettingsService : ISettingsModel
    {
        /// <summary>
        /// Determines whether to lock all unlocked vaults when the system is locked.
        /// </summary>
        bool AutoLockVaults { get; set; }

        /// <summary>
        /// Determines whether to use telemetry.
        /// </summary>
        bool IsTelemetryEnabled { get; set; }
    }
}
