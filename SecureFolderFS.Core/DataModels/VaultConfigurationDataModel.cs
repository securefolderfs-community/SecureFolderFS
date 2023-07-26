using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using SecureFolderFS.Core.Cryptography.Enums;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    internal sealed class VaultConfigurationDataModel
    {
        /// <summary>
        /// Gets the version of the vault.
        /// </summary>
        [JsonPropertyName("version")]
        public required int Version { get; init; }

        /// <summary>
        /// Gets scheme type of the vault for content encryption.
        /// </summary>
        [JsonPropertyName("contentCipherScheme")]
        [DefaultValue(ContentCipherScheme.Undefined)]
        public required ContentCipherScheme ContentCipherScheme { get; init; }

        /// <summary>
        /// Gets scheme type of the vault for name encryption.
        /// </summary>
        [JsonPropertyName("filenameCipherScheme")]
        [DefaultValue(ContentCipherScheme.Undefined)]
        public required FileNameCipherScheme FileNameCipherScheme { get; init; }

        /// <summary>
        /// Gets the unique identifier of the vault represented by a GUID.
        /// </summary>
        [JsonPropertyName("vaultId")]
        public required string Id { get; init; } = string.Empty;

        /// <summary>
        /// Gets the information about the authentication method used for this vault.
        /// </summary>
        [JsonPropertyName("authMode")]
        public required string AuthMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets the HMAC-SHA256 hash of the payload.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; set; }
    }
}
