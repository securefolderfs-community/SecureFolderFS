using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage privacy related settings.
    /// </summary>
    public interface IPrivacySettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the value that determines whether to lock all unlocked vaults when the system is locked.
        /// </summary>
        bool AutoLockVaults { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to use telemetry.
        /// </summary>
        bool IsTelemetryEnabled { get; set; }
    }
}
