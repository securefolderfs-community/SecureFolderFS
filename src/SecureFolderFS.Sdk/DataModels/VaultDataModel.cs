using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public abstract record VaultStorageSourceDataModel(string StorageType);

    [Serializable]
    public sealed record VaultDataModel(
        string? DisplayName,
        DateTime? LastAccessDate,
        VaultStorageSourceDataModel StorageSource);
}
