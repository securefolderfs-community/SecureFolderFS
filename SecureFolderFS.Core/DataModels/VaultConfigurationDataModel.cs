using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using static SecureFolderFS.Core.Constants;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    internal sealed class VaultConfigurationDataModel : VersionDataModel
    {
        /// <summary>
        /// Gets scheme type of the vault for content encryption.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_CONTENT_CIPHER_ID)]
        [DefaultValue("")]
        public required string ContentCipherId { get; init; }

        /// <summary>
        /// Gets scheme type of the vault for name encryption.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_FILENAME_CIPHER_ID)]
        [DefaultValue("")]
        public required string FileNameCipherId { get; init; }

        ///// <summary>
        ///// Gets the specialization of the vault that hints how the user data should be handled.
        ///// </summary>
        //[JsonPropertyName(Associations.ASSOC_SPECIALIZATION)]
        //[DefaultValue("")]
        //public required string Specialization { get; init; } = string.Empty;

        /// <summary>
        /// Gets the information about the authentication method used for this vault.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_AUTHENTICATION)]
        [DefaultValue("")]
        public required string AuthenticationMethod { get; set; } = string.Empty;

        /// <summary>
        /// Gets the unique identifier of the vault represented by a GUID.
        /// </summary>
        [JsonPropertyName("vaultId")]
        [DefaultValue("")]
        public required string Id { get; init; } = string.Empty;

        /// <summary>
        /// Gets the HMAC-SHA256 hash of the payload.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; set; }
    }
}
