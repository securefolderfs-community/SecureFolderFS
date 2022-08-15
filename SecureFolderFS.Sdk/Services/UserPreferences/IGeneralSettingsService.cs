using System;
using SecureFolderFS.Sdk.Models;

namespace SecureFolderFS.Sdk.Services.UserPreferences
{
    /// <summary>
    /// A service to manage general app settings.
    /// </summary>
    public interface IGeneralSettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the date when the app last checked for updates.
        /// </summary>
        DateTime UpdateLastChecked { get; set; }
    }
}
