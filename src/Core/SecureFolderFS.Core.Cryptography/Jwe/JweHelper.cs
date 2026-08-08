using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Jose;

namespace SecureFolderFS.Core.Cryptography.Jwe
{
    /// <summary>
    /// Provides JWE encryption/decryption using ECDH-ES+A256KW key agreement with A256GCM content encryption.
    /// </summary>
    public static class JweHelper
    {
        /// <summary>
        /// Encrypts a byte payload for a recipient's EC P-256 public key, producing a JWE compact serialization.
        /// Includes a <c>kid</c> header (JWK Thumbprint, RFC 7638) binding the JWE to the recipient's key.
        /// </summary>
        /// <param name="plaintext">The plaintext bytes to encrypt.</param>
        /// <param name="recipientPublicKey">The recipient's EC P-256 public key (only the public component is used).</param>
        /// <param name="extraHeaders">Optional additional JWE headers to include.</param>
        /// <returns>A JWE compact serialization string.</returns>
        public static string Encrypt(byte[] plaintext, ECDiffieHellman recipientPublicKey, IDictionary<string, object>? extraHeaders = null)
        {
            return JWT.EncodeBytes(plaintext, recipientPublicKey, JweAlgorithm.ECDH_ES_A256KW, JweEncryption.A256GCM, extraHeaders: extraHeaders);
        }

        /// <summary>
        /// Encrypts a byte payload for a recipient identified by their public key JWK string.
        /// Includes a <c>kid</c> header (JWK Thumbprint, RFC 7638) to cryptographically bind the JWE
        /// to the intended recipient's public key. The server uses this to verify the JWE is encrypted
        /// for the correct user.
        /// </summary>
        /// <param name="plaintext">The plaintext bytes to encrypt.</param>
        /// <param name="recipientPublicKeyJwk">The recipient's public key as a JWK JSON string.</param>
        /// <returns>A JWE compact serialization string.</returns>
        public static string Encrypt(byte[] plaintext, string recipientPublicKeyJwk)
        {
            using var publicKey = EcKeyHelper.ImportPublicKeyJwk(recipientPublicKeyJwk);
            var kid = EcKeyHelper.ComputeJwkThumbprint(recipientPublicKeyJwk);
            var headers = new Dictionary<string, object> { ["kid"] = kid };
            return Encrypt(plaintext, publicKey, headers);
        }

        /// <summary>
        /// Decrypts a JWE compact serialization using the recipient's EC P-256 private key.
        /// </summary>
        /// <param name="jweCompact">The JWE compact serialization string to decrypt.</param>
        /// <param name="recipientPrivateKey">The recipient's EC P-256 private key.</param>
        /// <returns>The decrypted plaintext bytes.</returns>
        public static byte[] Decrypt(string jweCompact, ECDiffieHellman recipientPrivateKey)
        {
            return JWT.DecodeBytes(jweCompact, recipientPrivateKey, JweAlgorithm.ECDH_ES_A256KW, JweEncryption.A256GCM);
        }

        /// <summary>
        /// Decrypts a JWE compact serialization using a private key loaded from raw bytes.
        /// </summary>
        /// <param name="jweCompact">The JWE compact serialization string to decrypt.</param>
        /// <param name="recipientPrivateKeyBytes">The recipient's private key as DER-encoded bytes.</param>
        /// <returns>The decrypted plaintext bytes.</returns>
        public static byte[] Decrypt(string jweCompact, byte[] recipientPrivateKeyBytes)
        {
            using var privateKey = EcKeyHelper.ImportPrivateKeyBytes(recipientPrivateKeyBytes);
            return Decrypt(jweCompact, privateKey);
        }

        /// <summary>
        /// Encrypts a vault key (DEK + MAC concatenated) for a recipient, producing a JWE.
        /// </summary>
        /// <param name="dekKey">The 32-byte Data Encryption Key.</param>
        /// <param name="macKey">The 32-byte Message Authentication Code key.</param>
        /// <param name="recipientPublicKeyJwk">The recipient's public key as a JWK JSON string.</param>
        /// <returns>A JWE compact serialization containing the encrypted vault key material.</returns>
        public static string EncryptVaultKey(ReadOnlySpan<byte> dekKey, ReadOnlySpan<byte> macKey, string recipientPublicKeyJwk)
        {
            var combined = new byte[dekKey.Length + macKey.Length];
            try
            {
                dekKey.CopyTo(combined);
                macKey.CopyTo(combined.AsSpan(dekKey.Length));
                return Encrypt(combined, recipientPublicKeyJwk);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(combined);
            }
        }

        /// <summary>
        /// Decrypts a JWE containing a vault key and splits it into DEK and MAC components.
        /// </summary>
        /// <param name="jweCompact">The JWE compact serialization containing the encrypted vault key.</param>
        /// <param name="recipientPrivateKey">The recipient's EC P-256 private key.</param>
        /// <returns>A tuple of (dekKey, macKey) byte arrays. Caller is responsible for zeroing these when done.</returns>
        public static (byte[] dekKey, byte[] macKey) DecryptVaultKey(string jweCompact, ECDiffieHellman recipientPrivateKey)
        {
            var combined = Decrypt(jweCompact, recipientPrivateKey);
            try
            {
                if (combined.Length != Constants.KeyTraits.DEK_KEY_LENGTH + Constants.KeyTraits.MAC_KEY_LENGTH)
                    throw new CryptographicException($"Decrypted vault key has unexpected length: {combined.Length}");

                var dekKey = new byte[Constants.KeyTraits.DEK_KEY_LENGTH];
                var macKey = new byte[Constants.KeyTraits.MAC_KEY_LENGTH];

                combined.AsSpan(0, Constants.KeyTraits.DEK_KEY_LENGTH).CopyTo(dekKey);
                combined.AsSpan(Constants.KeyTraits.DEK_KEY_LENGTH, Constants.KeyTraits.MAC_KEY_LENGTH).CopyTo(macKey);

                return (dekKey, macKey);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(combined);
            }
        }
    }
}
