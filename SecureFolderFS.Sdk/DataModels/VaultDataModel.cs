using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record class VaultDataModel(string? Id, string? VaultName, DateTime? LastAccessDate)
    {
        [JsonPropertyName("Name")]
        public string? VaultName { get; set; } = VaultName;

        [JsonPropertyName("LastAccessDate")]
        public DateTime? LastAccessDate { get; set; } = LastAccessDate;
    }
}
