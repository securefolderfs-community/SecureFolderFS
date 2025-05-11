using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using static SecureFolderFS.Core.Constants.Vault;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public sealed record class VaultConfigurationDataModel : VersionDataModel
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
        /// Gets the ID for file name encoding.
        /// </summary>
        [JsonPropertyName(Associations.ASSOC_FILENAME_ENCODING_ID)]
        [DefaultValue("")]
        public string FileNameEncodingId { get; set; } = Cryptography.Constants.CipherId.ENCODING_BASE64URL;

        /// <summary>
        /// Gets the size of the recycle bin.
        /// </summary>
        /// <remarks>
        /// If the size is zero, the recycle bin is disabled.
        /// If the size is any value smaller than zero, the recycle bin has unlimited size capacity.
        /// Any values above zero indicate the maximum capacity in bytes that is allowed for the recycling operation to proceed.
        /// </remarks>
        [JsonPropertyName(Associations.ASSOC_RECYCLE_SIZE)]
        [DefaultValue(0L)]
        public long RecycleBinSize { get; set; } = 0L;

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
