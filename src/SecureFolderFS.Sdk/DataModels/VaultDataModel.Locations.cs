using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record LocalStorageSourceDataModel() : VaultStorageSourceDataModel("LocalStorage");

    [Serializable]
    public sealed record AccountSourceDataModel(string? AccountId) : VaultStorageSourceDataModel("RemoteAccountStorage");
}
