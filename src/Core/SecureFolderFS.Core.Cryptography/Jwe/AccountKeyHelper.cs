using System;
using System.Security.Cryptography;
using Jose;

namespace SecureFolderFS.Core.Cryptography.Jwe
{
    /// <summary>
    /// Provides PBES2-based JWE operations for Account Key (passphrase) wrapping of EC private keys.
    /// Used to bootstrap new devices when no device-specific JWE exists yet.
    /// </summary>
    public static class AccountKeyHelper
    {
        /// <summary>
        /// Wraps an EC private key (in DER format) under a user-provided passphrase using PBES2-HS256+A128KW / A256GCM.
        /// </summary>
        /// <param name="privateKeyBytes">The EC private key bytes (DER-encoded) to wrap.</param>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <returns>A JWE compact serialization string containing the encrypted private key.</returns>
        public static string Wrap(byte[] privateKeyBytes, string passphrase)
        {
            return JWT.EncodeBytes(privateKeyBytes, passphrase, JweAlgorithm.PBES2_HS256_A128KW, JweEncryption.A256GCM);
        }

        /// <summary>
        /// Unwraps an EC private key from a PBES2-protected JWE using the Account Key passphrase.
        /// </summary>
        /// <param name="jweCompact">The JWE compact serialization containing the wrapped private key.</param>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <returns>The EC private key bytes (DER-encoded).</returns>
        public static byte[] Unwrap(string jweCompact, string passphrase)
        {
            return JWT.DecodeBytes(jweCompact, passphrase, JweAlgorithm.PBES2_HS256_A128KW, JweEncryption.A256GCM);
        }

        /// <summary>
        /// Wraps a user's EC private key for Account Key bootstrap.
        /// The private key is stored in JWK format inside the JWE for cross-platform compatibility.
        /// </summary>
        /// <param name="userPrivateKey">The user's EC private key to wrap.</param>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <returns>A JWE compact serialization containing the encrypted user private key (as JWK).</returns>
        public static string WrapUserKey(ECDiffieHellman userPrivateKey, string passphrase)
        {
            var privateKeyJwk = EcKeyHelper.ExportPrivateKeyJwk(userPrivateKey);
            var privateKeyBytes = System.Text.Encoding.UTF8.GetBytes(privateKeyJwk);
            try
            {
                return Wrap(privateKeyBytes, passphrase);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }

        /// <summary>
        /// Unwraps a user's EC private key from an Account Key-protected JWE.
        /// Expects the JWE to contain the private key in JWK format.
        /// </summary>
        /// <param name="jweCompact">The JWE compact serialization containing the wrapped user private key.</param>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <returns>An <see cref="ECDiffieHellman"/> instance with the decrypted user private key.</returns>
        public static ECDiffieHellman UnwrapUserKey(string jweCompact, string passphrase)
        {
            var privateKeyBytes = Unwrap(jweCompact, passphrase);
            try
            {
                var jwk = System.Text.Encoding.UTF8.GetString(privateKeyBytes);
                return EcKeyHelper.ImportPrivateKeyJwk(jwk);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(privateKeyBytes);
            }
        }
    }
}
