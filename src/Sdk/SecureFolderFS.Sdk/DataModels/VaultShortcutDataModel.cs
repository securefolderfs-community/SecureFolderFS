using System;

namespace SecureFolderFS.Sdk.DataModels
{
    /// <summary>
    /// Represents the data stored in a .sfvault shortcut file.
    /// </summary>
    [Serializable]
    public sealed record VaultShortcutDataModel(
        /// <summary>
        /// Gets the persistable ID of the vault folder.
        /// </summary>
        string? PersistableId,

        /// <summary>
        /// Gets the display name of the vault.
        /// </summary>
        string? VaultName,

        /// <summary>
        /// Gets the full path to the vault folder (for fallback resolution).
        /// </summary>
        string? VaultPath
    );
}

