using System;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record LocalStorageSourceDataModel(string Location) : VaultStorageSourceDataModel("STORAGE_LOCAL");

    [Serializable]
    public sealed record FtpStorageSourceDataModel(string Host, int Port, string RemoteDirectory, string? OpaqueKey) : VaultStorageSourceDataModel("STORAGE_FTP");
}
