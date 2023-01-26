using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service for storing application configuration and settings.
    /// </summary>
    public interface IApplicationSettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the id associated with vault folder.
        /// </summary>
        string? LastVaultFolderId { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether Out of Box Experience was shown.
        /// </summary>
        bool IsIntroduced { get; set; }
    }
}
