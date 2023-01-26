using SecureFolderFS.Sdk.Models;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.Services
{
    internal interface IPlatformSettingsService : ISettingsModel
    {
        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        public ApplicationTheme Theme { get; set; }
    }
}