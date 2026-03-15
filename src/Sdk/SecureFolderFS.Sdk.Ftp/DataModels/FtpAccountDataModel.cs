using System;
using System.Text.Json.Serialization;
using SecureFolderFS.Sdk.Accounts.DataModels;

namespace SecureFolderFS.Sdk.Ftp.DataModels
{
    [Serializable]
    public sealed record FtpAccountDataModel(string? AccountId, string? DataSourceType, string? DisplayName) : AccountDataModel(AccountId, DataSourceType, DisplayName)
    {
        [JsonPropertyName("address")]
        public required string? Address { get; init; }

        [JsonPropertyName("username")]
        public required string? UserName { get; init; }

        [JsonPropertyName("password")]
        public required string? Password { get; init; }
    }
}
