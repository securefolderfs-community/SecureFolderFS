using System;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.Migration.AppModels.V1_V2
{
    [Serializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal sealed class V1VaultConfigurationDataModel
    {
        /// <summary>
        /// Gets scheme type of the vault for content encryption.
        /// </summary>
        [JsonPropertyName("contentCipherScheme")]
        [DefaultValue(0)]
        public int ContentCipherScheme { get; init; }

        /// <summary>
        /// Gets scheme type of the vault for name encryption.
        /// </summary>
        [JsonPropertyName("filenameCipherScheme")]
        [DefaultValue(0)]
        public int FileNameCipherScheme { get; init; }

        /// <summary>
        /// Gets the version of the vault.
        /// </summary>
        [JsonPropertyName("version")]
        public int Version { get; init; }

        /// <summary>
        /// Gets the HMAC-SHA256 hash of the payload.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; init; }
    }
}
