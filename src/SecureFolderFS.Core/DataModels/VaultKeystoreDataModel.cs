using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public sealed class VaultKeystoreDataModel
    {
        /// <summary>
        /// Gets wrapped version stored in keystore of the encryption key.
        /// </summary>
        [JsonPropertyName("c_encryptionKey")]
        public byte[]? WrappedEncKey { get; init; }

        /// <summary>
        /// Gets wrapped version stored in keystore of the mac key.
        /// </summary>
        [JsonPropertyName("c_macKey")]
        public byte[]? WrappedMacKey { get; init; }

        /// <summary>
        /// Gets the salt used during password hashing.
        /// </summary>
        [JsonPropertyName("salt")]
        public byte[]? Salt { get; init; }
    }
}
