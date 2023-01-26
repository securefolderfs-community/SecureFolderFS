using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage settings of user preferences.
    /// </summary>
    public interface IPreferencesSettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the value that determines the type of the preferred file system provider to use.
        /// </summary>
        string? PreferredFileSystemId { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to launch SecureFolderFS on system startup.
        /// </summary>
        bool StartOnSystemStartup { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to continue on the previously selected vault.
        /// </summary>
        bool ContinueOnLastVault { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to open the vault root folder when it is unlocked.
        /// </summary>
        bool OpenFolderOnUnlock { get; set; }
    }
}
