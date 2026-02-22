using System;

namespace SecureFolderFS.Sdk.DataModels
{
    /// <summary>
    /// Represents the data stored in a .sfvault shortcut file.
    /// </summary>
    /// <param name="PersistableId">Gets the persistable ID of the vault folder.</param>
    /// <param name="VaultName">Gets the display name of the vault.</param>
    [Serializable]
    public sealed record VaultShortcutDataModel(string? PersistableId, string? VaultName);
}

