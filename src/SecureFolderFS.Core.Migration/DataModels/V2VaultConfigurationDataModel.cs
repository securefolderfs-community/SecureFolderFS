using SecureFolderFS.Core.DataModels;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.Core.Migration.DataModels
{
    [Serializable]
    public sealed class V2VaultConfigurationDataModel : VersionDataModel
    {
        /// <summary>
        /// Gets the ID for content encryption.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_CONTENT_CIPHER_ID)]
        [DefaultValue("")]
        public required string ContentCipherId { get; init; }

        /// <summary>
        /// Gets the ID for file name encryption.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_FILENAME_CIPHER_ID)]
        [DefaultValue("")]
        public required string FileNameCipherId { get; init; }

        /// <summary>
        /// Gets the information about the authentication method used for this vault.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_AUTHENTICATION)]
        [DefaultValue("")]
        public required string AuthenticationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets the unique identifier of the vault represented by a GUID.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_VAULT_ID)]
        [DefaultValue("")]
        public required string Uid { get; init; } = string.Empty;

        /// <summary>
        /// Gets the HMAC-SHA256 hash of the payload.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; set; }
    }
}
