using SecureFolderFS.Core.Cryptography.Cipher;
using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;

namespace SecureFolderFS.Core.DataModels
{
    [Serializable]
    internal class VaultAuthenticationDataModel
    {
        /// <summary>
        /// Gets the capability of the authentication method.
        /// </summary>
        /// <remarks>
        /// The capability informs how the properties of the authentication data model
        /// should be interpreted and to what capacity this attestation can be used.
        /// </remarks>
        [JsonPropertyName("capability")] // supportsVersion|supports..
        [DefaultValue("")]
        public string Capability { get; init; } = string.Empty;

        /// <summary>
        /// Gets the HMAC-SHA256 hash of the payload.
        /// </summary>
        [JsonPropertyName("hmacsha256mac")]
        public byte[]? PayloadMac { get; set; }

        /// <summary>
        /// Computes a unique HMAC thumbprint of the properties of this class.
        /// </summary>
        /// <param name="macKey">The key part of HMAC.</param>
        /// <param name="hmacSha256Crypt">The HMAC-SHA256 cryptographic provider.</param>
        /// <param name="mac">The destination to fill the calculated HMAC thumbprint into.</param>
        public void CalculateAuthMac(SecretKey macKey, IHmacSha256Crypt hmacSha256Crypt, Span<byte> mac)
        {
            // Initialize HMAC
            using var hmacSha256 = hmacSha256Crypt.GetInstance();
            hmacSha256.InitializeHmac(macKey);

            // We rely on the implementation to update the HMAC
            UpdateHmac(hmacSha256);

            // Fill the hash to payload
            hmacSha256.GetHash(mac);
        }

        /// <summary>
        /// Updates the <see cref="hmacSha256"/> instance with the properties of this class.
        /// </summary>
        /// <param name="hmacSha256">The HMAC instance.</param>
        protected virtual void UpdateHmac(IHmacSha256Instance hmacSha256)
        {
            // Update HMAC
            hmacSha256.Update(Encoding.UTF8.GetBytes(Capability));  // Capability
        }
    }
}
