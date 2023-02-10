using SecureFolderFS.Shared.Utils;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// An interface to manage user preferences and settings.
    /// </summary>
    public interface IUserSettings : IPersistable, INotifyPropertyChanged
    {
        #region Privacy Settings

        /// <summary>
        /// Gets or sets the value that determines whether to lock all unlocked vaults when the system is locked.
        /// </summary>
        bool AutoLockVaults { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to use telemetry.
        /// </summary>
        bool IsTelemetryEnabled { get; set; }

        #endregion

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
        /// Gets or sets the value that determines whether to continue on the previously selected vault.
        /// </summary>
        bool ContinueOnLastVault { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether to open the vault root folder when it is unlocked.
        /// </summary>
        bool OpenFolderOnUnlock { get; set; }

        #endregion
    }
}
