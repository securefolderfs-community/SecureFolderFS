using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace SecureFolderFS.Core.Cryptography.Jwe
{
    /// <summary>
    /// Provides EC P-256 key pair generation, JWK serialization, and import/export operations.
    /// </summary>
    public static class EcKeyHelper
    {
        /// <summary>
        /// Generates a new EC P-256 key pair for ECDH key agreement.
        /// </summary>
        public static ECDiffieHellman GenerateKeyPair()
        {
            return ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        }

        /// <summary>
        /// Exports the public key of an <see cref="ECDiffieHellman"/> instance as a JWK JSON string.
        /// </summary>
        /// <param name="key">The key pair to export the public component from.</param>
        /// <returns>A JSON string in JWK format containing the public key.</returns>
        public static string ExportPublicKeyJwk(ECDiffieHellman key)
        {
            var parameters = key.ExportParameters(includePrivateParameters: false);
            return SerializeJwk(parameters, includePrivate: false);
        }

        /// <summary>
        /// Exports the full key pair (public + private) as a JWK JSON string.
        /// </summary>
        /// <param name="key">The key pair to export.</param>
        /// <returns>A JSON string in JWK format containing both public and private key components.</returns>
        public static string ExportPrivateKeyJwk(ECDiffieHellman key)
        {
            var parameters = key.ExportParameters(includePrivateParameters: true);
            return SerializeJwk(parameters, includePrivate: true);
        }

        /// <summary>
        /// Exports the private key as a DER-encoded byte array suitable for secure storage.
        /// </summary>
        /// <param name="key">The key pair to export the private key from.</param>
        /// <returns>A byte array containing the private key in SEC1/ECPrivateKey format.</returns>
        public static byte[] ExportPrivateKeyBytes(ECDiffieHellman key)
        {
            return key.ExportECPrivateKey();
        }

        /// <summary>
        /// Imports an EC P-256 public key from a JWK JSON string.
        /// </summary>
        /// <param name="jwk">The JWK JSON string containing the public key.</param>
        /// <returns>An <see cref="ECDiffieHellman"/> instance with only the public key component.</returns>
        public static ECDiffieHellman ImportPublicKeyJwk(string jwk)
        {
            var parameters = DeserializeJwk(jwk);
            parameters.D = null;
            var ecdh = ECDiffieHellman.Create();
            ecdh.ImportParameters(parameters);
            return ecdh;
        }

        /// <summary>
        /// Imports an EC P-256 key pair from a JWK JSON string that includes the private key.
        /// </summary>
        /// <param name="jwk">The JWK JSON string containing both public and private key components.</param>
        /// <returns>An <see cref="ECDiffieHellman"/> instance with both public and private key components.</returns>
        public static ECDiffieHellman ImportPrivateKeyJwk(string jwk)
        {
            var parameters = DeserializeJwk(jwk);
            var ecdh = ECDiffieHellman.Create();
            ecdh.ImportParameters(parameters);
            return ecdh;
        }

        /// <summary>
        /// Imports a private key from a DER-encoded byte array (SEC1/ECPrivateKey format).
        /// </summary>
        /// <param name="privateKeyBytes">The DER-encoded private key bytes.</param>
        /// <returns>An <see cref="ECDiffieHellman"/> instance with the imported private key.</returns>
        public static ECDiffieHellman ImportPrivateKeyBytes(byte[] privateKeyBytes)
        {
            var ecdh = ECDiffieHellman.Create();
            ecdh.ImportECPrivateKey(privateKeyBytes, out _);
            return ecdh;
        }

        /// <summary>
        /// Compares the public EC coordinates in two P-256 JWKs.
        /// </summary>
        public static bool PublicJwksEqual(string leftJwk, string rightJwk)
        {
            var left = DeserializeJwk(leftJwk);
            var right = DeserializeJwk(rightJwk);

            return left.Q.X is not null &&
                   left.Q.Y is not null &&
                   right.Q.X is not null &&
                   right.Q.Y is not null &&
                   CryptographicOperations.FixedTimeEquals(left.Q.X, right.Q.X) &&
                   CryptographicOperations.FixedTimeEquals(left.Q.Y, right.Q.Y);
        }

        /// <summary>
        /// Computes the JWK Thumbprint (RFC 7638) for an EC P-256 public key JWK.
        /// Uses SHA-256 over the lexicographically-sorted required members: crv, kty, x, y.
        /// </summary>
        /// <param name="publicKeyJwk">The public key as a JWK JSON string.</param>
        /// <returns>A base64url-encoded SHA-256 thumbprint.</returns>
        public static string ComputeJwkThumbprint(string publicKeyJwk)
        {
            using var doc = JsonDocument.Parse(publicKeyJwk);
            var root = doc.RootElement;

            var crv = root.GetProperty("crv").GetString();
            var kty = root.GetProperty("kty").GetString();
            var x = root.GetProperty("x").GetString();
            var y = root.GetProperty("y").GetString();

            // RFC 7638: canonical JSON with required members in lexicographic order
            // For EC keys the required members are: crv, kty, x, y
            var canonical = $"{{\"crv\":\"{crv}\",\"kty\":\"{kty}\",\"x\":\"{x}\",\"y\":\"{y}\"}}";
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
            return Base64UrlEncode(hash);
        }

        private static string SerializeJwk(ECParameters parameters, bool includePrivate)
        {
            using var stream = new System.IO.MemoryStream();
            using (var writer = new Utf8JsonWriter(stream))
            {
                writer.WriteStartObject();
                writer.WriteString("kty", "EC");
                writer.WriteString("crv", "P-256");
                writer.WriteString("x", Base64UrlEncode(parameters.Q.X!));
                writer.WriteString("y", Base64UrlEncode(parameters.Q.Y!));

                if (includePrivate && parameters.D is not null)
                    writer.WriteString("d", Base64UrlEncode(parameters.D));

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(stream.ToArray());
        }

        private static ECParameters DeserializeJwk(string jwk)
        {
            using var doc = JsonDocument.Parse(jwk);
            var root = doc.RootElement;

            var kty = root.GetProperty("kty").GetString();
            var crv = root.GetProperty("crv").GetString();

            if (kty != "EC" || crv != "P-256")
                throw new CryptographicException($"Unsupported JWK key type or curve: kty={kty}, crv={crv}");

            var parameters = new ECParameters
            {
                Curve = ECCurve.NamedCurves.nistP256,
                Q = new ECPoint
                {
                    X = Base64UrlDecode(root.GetProperty("x").GetString()!),
                    Y = Base64UrlDecode(root.GetProperty("y").GetString()!)
                }
            };

            if (root.TryGetProperty("d", out var dElement) && dElement.GetString() is { } dValue)
                parameters.D = Base64UrlDecode(dValue);

            return parameters;
        }

        private static string Base64UrlEncode(byte[] data)
        {
            return Convert.ToBase64String(data)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');
        }

        private static byte[] Base64UrlDecode(string base64Url)
        {
            var s = base64Url.Replace('-', '+').Replace('_', '/');
            if (s.Length % 4 == 1)
                throw new FormatException("Invalid base64url length.");

            switch (s.Length % 4)
            {
                case 2: s += "=="; break;
                case 3: s += "="; break;
            }

            return Convert.FromBase64String(s);
        }
    }
}
