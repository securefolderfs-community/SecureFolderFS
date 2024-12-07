using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// An interface to manage user preferences and settings.
    /// </summary>
    public interface IUserSettings : IPersistable, INotifyPropertyChanged
    {
        #region Preferences

        /// <summary>
        /// Gets or sets the value that determines the type of the preferred file system provider to use.
        /// </summary>
        string PreferredFileSystemId { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to launch SecureFolderFS on system startup.
        /// </summary>
        bool StartOnSystemStartup { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to put the app to background system Task Bar when closing the window.
        /// </summary>
        bool ReduceToBackground { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to continue on the previously selected vault.
        /// </summary>
        bool ContinueOnLastVault { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to open the vault root folder when it is unlocked.
        /// </summary>
        bool OpenFolderOnUnlock { get; set; }

        #endregion

        #region Privacy

        /// <summary>
        /// Gets or sets the value that determines whether to lock all unlocked vaults when the system is locked.
        /// </summary>
        bool LockOnSystemLock { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to use telemetry.
        /// </summary>
        bool IsTelemetryEnabled { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to periodically clear or untrack system-wide recently accessed items.
        /// </summary>
        bool DisableRecentAccess { get; set; }

        #endregion
    }
}
