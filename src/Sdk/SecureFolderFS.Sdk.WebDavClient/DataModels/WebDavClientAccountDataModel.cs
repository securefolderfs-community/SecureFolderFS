using System;
using System.Text.Json.Serialization;
using SecureFolderFS.Sdk.Accounts.DataModels;

namespace SecureFolderFS.Sdk.WebDavClient.DataModels
{
    [Serializable]
    public sealed record WebDavClientAccountDataModel(string? AccountId, string? DataSourceType, string? DisplayName) : AccountDataModel(AccountId, DataSourceType, DisplayName)
    {
        [JsonPropertyName("address")]
        public required string? Address { get; init; }

        [JsonPropertyName("port")]
        public required string? Port { get; init; }

        [JsonPropertyName("username")]
        public required string? UserName { get; init; }

        [JsonPropertyName("password")]
        public required string? Password { get; init; }

        [JsonPropertyName("acceptFirstCertificate")]
        public bool AcceptFirstCertificate { get; init; }

        [JsonPropertyName("manualCertificateFingerprint")]
        public string? ManualCertificateFingerprint { get; init; }

        [JsonPropertyName("trustedCertificateFingerprint")]
        public string? TrustedCertificateFingerprint { get; init; }
    }
}

