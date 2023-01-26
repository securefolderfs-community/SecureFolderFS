using SecureFolderFS.Core.Cryptography.Enums;
using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    internal sealed class VaultConfigurationDataModel
    {
        /// <summary>
        /// Gets scheme type of the vault for content encryption.
        /// </summary>
        [JsonPropertyName("contentCipherScheme")]
        [DefaultValue(ContentCipherScheme.Undefined)]
        public ContentCipherScheme ContentCipherScheme { get; init; }

        /// <summary>
        /// Gets scheme type of the vault for name encryption.
        /// </summary>
        [JsonPropertyName("filenameCipherScheme")]
        [DefaultValue(ContentCipherScheme.Undefined)]
        public FileNameCipherScheme FileNameCipherScheme { get; init; }

        /// <summary>
        /// Gets the version of the vault.
        /// </summary>
        [JsonPropertyName("version")]
        [DefaultValue(Constants.VaultVersion.LATEST_VERSION)]
        public int Version { get; init; } = Constants.VaultVersion.LATEST_VERSION;

        /// <summary>
        /// Gets the HMAC-SHA256 hash of the payload.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; init; }
    }
}
