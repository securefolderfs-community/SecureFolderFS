using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Shared.Models
{
    /// <summary>
    /// Stores broker metadata required to resolve vault keys through App Platform.
    /// </summary>
    [Serializable]
    public sealed record class AppPlatformVaultOptions
    {
        [JsonPropertyName("serverUrl")]
        public required string ServerUrl { get; init; }

        [JsonPropertyName("vaultResource")]
        public required string VaultResource { get; init; }

        [JsonPropertyName("organization")]
        public string? Organization { get; init; }

        [JsonPropertyName("accessTokenEndpoint")]
        public string AccessTokenEndpoint { get; init; } = "/api/app-platform/access-token";

        [JsonPropertyName("deviceRegistrationEndpoint")]
        public string DeviceRegistrationEndpoint { get; init; } = "/api/app-platform/devices";
    }
}

