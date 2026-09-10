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
    }
}

