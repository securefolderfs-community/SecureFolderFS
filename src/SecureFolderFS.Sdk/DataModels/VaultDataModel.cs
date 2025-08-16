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
        public string? DisplayName { get; set; } = DisplayName;

        public DateTime? LastAccessDate { get; set; } = LastAccessDate;

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
