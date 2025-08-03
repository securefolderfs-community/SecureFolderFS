using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record LocalStorageSourceDataModel(string PersistableId) : VaultStorageSourceDataModel("LocalStorage");

    [Serializable]
    public sealed record AccountSourceDataModel(string AccountId, string PersistableId) : VaultStorageSourceDataModel("RemoteAccountStorage");
}
