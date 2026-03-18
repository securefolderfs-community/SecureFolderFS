using System;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    public sealed record class V4VaultKeystoreDataModel
    {
        /// <summary>
        /// Gets the wrapped version of the DEK key.
        /// </summary>
        [JsonPropertyName("c_encryptionKey")]
        public byte[]? WrappedDekKey { get; init; }

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

        /// <summary>
        /// Gets the AES-256-GCM ciphertext of the 256-bit SoftwareEntropy value.
        /// SoftwareEntropy is a CSPRNG secret generated at vault creation that is mixed
        /// into the Argon2id input via HKDF-Extract, raising the quantum security floor
        /// of all authentication methods to 256 bits regardless of auth factor entropy.
        /// It is encrypted under a key derived from the passkey so all active auth
        /// factors are required to recover it.
        /// </summary>
        [JsonPropertyName("c_softwareEntropy")]
        public byte[]? EncryptedSoftwareEntropy { get; init; }

        /// <summary>
        /// Gets the nonce used when encrypting <see cref="EncryptedSoftwareEntropy"/>.
        /// </summary>
        [JsonPropertyName("entropyNonce")]
        public byte[]? SoftwareEntropyNonce { get; init; }

        /// <summary>
        /// Gets the AES-256-GCM authentication tag for <see cref="EncryptedSoftwareEntropy"/>.
        /// </summary>
        [JsonPropertyName("entropyTag")]
        public byte[]? SoftwareEntropyTag { get; init; }
    }
}