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
        /// <summary>
        /// Iteration count used when wrapping.
        /// </summary>
        /// <remarks>
        /// Unwrap accepts [<see cref="MIN_PBES2_ITERATIONS"/>,
        /// <see cref="MAX_PBES2_ITERATIONS"/>] so this can be raised without stranding existing account
        /// keys. Must stay in sync with crypto-interop.js (WASM) and JweFormatValidator (server).
        /// </remarks>
        private const int ACCOUNT_KEY_PBES2_ITERATIONS = 600_000;

        /// <summary>
        /// Lowest accepted iteration count (OWASP minimum for PBKDF2-HMAC-SHA512).
        /// </summary>
        private const int MIN_PBES2_ITERATIONS = 210_000;

        /// <summary>
        /// Highest accepted iteration count; bounds p2c amplification DoS.
        /// </summary>
        private const int MAX_PBES2_ITERATIONS = 1_000_000;

        /// <summary>
        /// Iteration count for the Account Key verifier derivation. Pinned independently of <see cref="ACCOUNT_KEY_PBES2_ITERATIONS"/>.
        /// </summary>
        /// <remarks>
        /// The verifier must derive to the same value for the
        /// lifetime of a registration, so changing this invalidates every stored verifier hash (users
        /// would have to re-register via setup or a passphrase change). Bump the context version string
        /// together with this value if it ever changes.
        /// </remarks>
        private const int ACCOUNT_VERIFIER_ITERATIONS = 600_000;

        /// <summary>
        /// Domain-separation context for the verifier derivation. Distinct from the PBES2 salt input
        /// ("PBES2-HS512+A256KW" || 0x00 || p2s) so the verifier is cryptographically independent of
        /// the JWE key-encryption key and cannot be used to unwrap the Account Key JWE.
        /// </summary>
        private const string ACCOUNT_VERIFIER_CONTEXT = "SFFS-account-verifier-v1";

        private const string ACCOUNT_KEY_ALG = "PBES2-HS512+A256KW";
        private const string ACCOUNT_KEY_ENC = "A256GCM";

        /// <summary>
        /// jose-jwt caps PBES2-HS512 p2c at 120,000 by default (its own amplification-DoS guard),
        /// which rejects our 600k wrap count. Re-register the key management with our accepted range
        /// so both bounds are enforced by the library on wrap and unwrap.
        /// </summary>
        private static readonly JwtSettings Pbes2Settings = new JwtSettings().RegisterJwa(
            JweAlgorithm.PBES2_HS512_A256KW,
            new Pbse2HmacShaKeyManagementWithAesKeyWrap(
                256, new AesKeyWrapManagement(256),
                maxIterations: MAX_PBES2_ITERATIONS,
                minIterations: MIN_PBES2_ITERATIONS));

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
                ["p2c"] = ACCOUNT_KEY_PBES2_ITERATIONS
            };

            return JWT.EncodeBytes(privateKeyBytes, passphrase, JweAlgorithm.PBES2_HS512_A256KW, JweEncryption.A256GCM, extraHeaders: headers, settings: Pbes2Settings);
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
            return JWT.DecodeBytes(jweCompact, passphrase, JweAlgorithm.PBES2_HS512_A256KW, JweEncryption.A256GCM, settings: Pbes2Settings);
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
                !string.Equals(Convert.ToString(alg, CultureInfo.InvariantCulture), ACCOUNT_KEY_ALG, StringComparison.Ordinal))
            {
                throw new CryptographicException("Unsupported Account Key JWE algorithm.");
            }

            if (!headers.TryGetValue("enc", out var enc) ||
                !string.Equals(Convert.ToString(enc, CultureInfo.InvariantCulture), ACCOUNT_KEY_ENC, StringComparison.Ordinal))
            {
                throw new CryptographicException("Unsupported Account Key JWE content encryption.");
            }

            if (!headers.TryGetValue("p2c", out var p2c) ||
                !TryConvertToInt64(p2c, out var iterations) ||
                iterations is < MIN_PBES2_ITERATIONS or > MAX_PBES2_ITERATIONS)
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
        /// Derives the Account Key verifier that facilitates passphrase-derived
        /// proof-of-possession token sent to the server in place of the raw passphrase.
        /// </summary>
        /// <remarks>
        /// The server stores only a hash of the token, so neither a
        /// compromised server nor a leaked database learns the passphrase or a value capable of
        /// unwrapping the Account Key JWE. Must produce byte-identical output to
        /// <c>deriveAccountVerifier</c> on the WASM end.
        /// </remarks>
        /// <param name="passphrase">The user-provided Account Key passphrase.</param>
        /// <param name="userId">The user's OIDC subject, used as a per-user salt component.</param>
        /// <returns>The verifier as a standard base64 string (32 bytes).</returns>
        public static string DeriveVerifier(string passphrase, string userId)
        {
            var contextBytes = System.Text.Encoding.UTF8.GetBytes(ACCOUNT_VERIFIER_CONTEXT);
            var userIdBytes = System.Text.Encoding.UTF8.GetBytes(userId);
            var salt = new byte[contextBytes.Length + 1 + userIdBytes.Length];
            contextBytes.CopyTo(salt, 0);
            salt[contextBytes.Length] = 0x00;
            userIdBytes.CopyTo(salt, contextBytes.Length + 1);

            var verifier = Rfc2898DeriveBytes.Pbkdf2(
                passphrase, salt, ACCOUNT_VERIFIER_ITERATIONS, HashAlgorithmName.SHA512, outputLength: 32);
            try
            {
                return Convert.ToBase64String(verifier);
            }
            finally
            {
                CryptographicOperations.ZeroMemory(verifier);
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
