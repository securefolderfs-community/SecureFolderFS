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
    }
}
