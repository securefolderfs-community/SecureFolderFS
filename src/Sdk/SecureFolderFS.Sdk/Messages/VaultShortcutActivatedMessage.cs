using SecureFolderFS.Sdk.DataModels;

namespace SecureFolderFS.Sdk.Messages
{
    /// <summary>
    /// A message notifying that a vault shortcut file was activated.
    /// </summary>
    public sealed class VaultShortcutActivatedMessage(VaultShortcutDataModel shortcutData, string? filePath = null)
    {
        /// <summary>
        /// Gets the shortcut data from the activated file.
        /// </summary>
        public VaultShortcutDataModel ShortcutData { get; } = shortcutData;

        /// <summary>
        /// Gets the path of the shortcut file that was activated.
        /// </summary>
        public string? FilePath { get; } = filePath;
    }
}

