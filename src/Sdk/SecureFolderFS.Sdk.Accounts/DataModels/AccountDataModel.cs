using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.Accounts.DataModels
{
    [Serializable]
    public record AccountDataModel(string? AccountId, string? DataSourceType, string? DisplayName)
    {
        [JsonPropertyName("accountId")]
        public string? AccountId { get; set; } = AccountId;

        [JsonPropertyName("dataSourceType")]
        public string? DataSourceType { get; set; } = DataSourceType;

        [JsonPropertyName("displayName")]
        public string? DisplayName { get; set; } = DisplayName;
    }
}
