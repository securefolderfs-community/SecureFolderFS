using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public sealed record class VaultKeystoreDataModel
    {
        /// <summary>
        /// Gets the wrapped version of the DEK key.
        /// </summary>
        [JsonPropertyName("c_encryptionKey")]
        public byte[]? WrappedEncKey { get; init; }

        /// <summary>
        /// Gets the wrapped version of the MAC key.
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
