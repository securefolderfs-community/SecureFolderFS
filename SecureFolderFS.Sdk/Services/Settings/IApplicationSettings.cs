using SecureFolderFS.Shared.Utils;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.Services.Settings
{
    /// <summary>
    /// An interface for storing application configuration and settings.
    /// </summary>
    public interface IApplicationSettings : IPersistable, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets the date when the app last checked for updates.
        /// </summary>
        DateTime UpdateLastChecked { get; set; }

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
