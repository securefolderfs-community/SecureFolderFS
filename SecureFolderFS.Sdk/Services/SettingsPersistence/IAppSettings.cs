using SecureFolderFS.Shared.Utils;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Services.SettingsPersistence
{
    /// <summary>
    /// An interface for storing application configuration and settings.
    /// </summary>
    public interface IAppSettings : IPersistable, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the value that determines whether the (first) notification about the beta program was shown.
        /// </summary>
        bool WasBetaNotificationShown1 { get; set; }

        /// <summary>
        /// Gets or sets the value that determines whether Out of Box Experience was shown.
        /// </summary>
        bool IsIntroduced { get; set; }

        /// <summary>
        /// Gets or sets the id associated with vault folder.
        /// </summary>
        string? LastVaultFolderId { get; set; }

        /// <summary>
        /// Gets or sets the value containing information about the app theme.
        /// </summary>
        string? ApplicationTheme { get; set; }

        /// <summary>
        /// Gets or sets the last version number used before an update.
        /// </summary>
        string? LastVersion { get; set; }

        /// <summary>
        /// Gets or sets the date when the app last checked for updates.
        /// </summary>
        DateTime UpdateLastChecked { get; set; }
    }
}
