using SecureFolderFS.Shared.Utils;
using SecureFolderFS.UI.Enums;

namespace SecureFolderFS.AvaloniaUI.Services
{
    internal interface IPlatformSettingsService : IPersistable
    {
        /// <summary>
        /// Gets or sets the application theme.
        /// </summary>
        public ApplicationTheme Theme { get; set; }
    }
}