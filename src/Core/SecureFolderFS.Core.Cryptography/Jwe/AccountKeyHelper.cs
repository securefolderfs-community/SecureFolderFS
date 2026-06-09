using System;
using System.Collections.Generic;
using System.Globalization;
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
        private const int AccountKeyPbes2Iterations = 210_000;
        private const string AccountKeyAlgorithm = "PBES2-HS512+A256KW";
        private const string AccountKeyEncryption = "A256GCM";

        /// <summary>
        /// Wraps an EC private key (in DER format) under a user-provided passphrase using PBES2-HS512+A256KW / A256GCM.
        /// Uses 256-bit AES key wrapping for post-quantum security margin.
        /// </summary>
        /// <param name="privateKeyBytes">The EC private key bytes (DER-encoded) to wrap.</param>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <returns>A JWE compact serialization string containing the encrypted private key.</returns>
        public static string Wrap(byte[] privateKeyBytes, string passphrase)
        {
            var headers = new Dictionary<string, object>
            {
                ["p2c"] = AccountKeyPbes2Iterations
            };

            return JWT.EncodeBytes(privateKeyBytes, passphrase, JweAlgorithm.PBES2_HS512_A256KW, JweEncryption.A256GCM, extraHeaders: headers);
        }

        /// <summary>
        /// Unwraps an EC private key from a PBES2-protected JWE using the Account Key passphrase.
        /// </summary>
        /// <param name="jweCompact">The JWE compact serialization containing the wrapped private key.</param>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <returns>The EC private key bytes (DER-encoded).</returns>
        public static byte[] Unwrap(string jweCompact, string passphrase)
        {
            ValidateAccountKeyHeader(jweCompact);
            return JWT.DecodeBytes(jweCompact, passphrase, JweAlgorithm.PBES2_HS512_A256KW, JweEncryption.A256GCM);
        }

        private static void ValidateAccountKeyHeader(string jweCompact)
        {
            IDictionary<string, object> headers;
            try
            {
                headers = JWT.Headers(jweCompact);
            }
            catch (Exception ex) when (ex is JoseException or ArgumentException or FormatException)
            {
                throw new CryptographicException("Invalid Account Key JWE header.", ex);
            }

            if (!headers.TryGetValue("alg", out var alg) ||
                !string.Equals(Convert.ToString(alg, CultureInfo.InvariantCulture), AccountKeyAlgorithm, StringComparison.Ordinal))
            {
                throw new CryptographicException("Unsupported Account Key JWE algorithm.");
            }

            if (!headers.TryGetValue("enc", out var enc) ||
                !string.Equals(Convert.ToString(enc, CultureInfo.InvariantCulture), AccountKeyEncryption, StringComparison.Ordinal))
            {
                throw new CryptographicException("Unsupported Account Key JWE content encryption.");
            }

            if (!headers.TryGetValue("p2c", out var p2c) ||
                !TryConvertToInt64(p2c, out var iterations) ||
                iterations != AccountKeyPbes2Iterations)
            {
                throw new CryptographicException("Unexpected Account Key PBES2 iteration count.");
            }
        }

        private static bool TryConvertToInt64(object value, out long result)
        {
            try
            {
                result = Convert.ToInt64(value, CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex) when (ex is FormatException or InvalidCastException or OverflowException)
            {
                result = 0;
                return false;
            }
        }

        /// <summary>
        /// Wraps a user's EC private key for Account Key bootstrap using PBES2-HS512+A256KW / A256GCM.
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
