using System;
using System.Text.Json.Serialization;
using SecureFolderFS.Sdk.Accounts.DataModels;

namespace SecureFolderFS.Sdk.GoogleDrive.DataModels
{
    [Serializable]
    public sealed record GDriveAccountDataModel(string? AccountId, string? DataSourceType, string? DisplayName): AccountDataModel(AccountId, DataSourceType, DisplayName)
    {
        [JsonPropertyName("userid")]
        public required string? UserId { get; init; }
    }
}