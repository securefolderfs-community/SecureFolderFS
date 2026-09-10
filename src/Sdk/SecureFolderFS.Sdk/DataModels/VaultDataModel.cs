using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public abstract record VaultStorageSourceDataModel(string? StorageType);

    [Serializable]
    public sealed record VaultDataModel(
        string? PersistableId,
        string? DisplayName,
        DateTime? LastAccessDate,
        VaultStorageSourceDataModel? StorageSource)
    {
        /// <summary>
        /// Gets or sets the display name of the vault.
        /// </summary>
        public string? DisplayName { get; set; } = DisplayName;

        /// <summary>
        /// Gets or sets the date and time when the vault was last accessed.
        /// </summary>
        public DateTime? LastAccessDate { get; set; } = LastAccessDate;

        /// <summary>
        /// Gets or sets the user-specified, per-vault file system options applied when the vault is mounted.
        /// </summary>
        public FileSystemOptionsDataModel? FileSystemOptions { get; set; }

        /// <inheritdoc/>
        public bool Equals(VaultDataModel? other)
        {
            if (other is null)
                return false;

            return (PersistableId?.Equals(other.PersistableId) ?? false)
                && (StorageSource?.Equals(other.StorageSource) ?? false);
        }
    }
}
