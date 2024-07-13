using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Sdk.DataModels
{
    [Serializable]
    public sealed record VaultDataModel(string? PersistableId, string? VaultName, DateTime? LastAccessDate)
    {
        [JsonPropertyName("Id")]
        public string? PersistableId { get; set; } = PersistableId;

        [JsonPropertyName("Name")]
        public string? VaultName { get; set; } = VaultName;

        [JsonPropertyName("LastAccessDate")]
        public DateTime? LastAccessDate { get; set; } = LastAccessDate;
    }
}
