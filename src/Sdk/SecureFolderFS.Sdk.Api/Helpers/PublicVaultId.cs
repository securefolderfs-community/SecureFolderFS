using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

namespace SecureFolderFS.Sdk.Api.Helpers
{
    /// <summary>
    /// Derives the public identifier under which a vault is exposed to integrations, so third-party
    /// configuration never comes to depend on the app's internal persistence identifier.
    /// </summary>
    public static class PublicVaultId
    {
        // 16 digest bytes equating to a 128-bit identifier
        private const int ID_BYTE_LENGTH = 16;

        /// <summary>
        /// Computes the public identifier for a vault.
        /// </summary>
        /// <param name="installSecret">The per-installation secret held by the pairing store.</param>
        /// <param name="persistableId">The application's internal identifier for the vault.</param>
        [SkipLocalsInit]
        public static string Compute(ReadOnlySpan<byte> installSecret, string persistableId)
        {
            Span<byte> digest = stackalloc byte[32];
            HMACSHA256.HashData(installSecret, Encoding.UTF8.GetBytes(persistableId), digest);

            return ToBase64Url(digest[..ID_BYTE_LENGTH]);
        }

        /// <summary>
        /// Encodes bytes using the URL-safe Base64 alphabet without padding.
        /// </summary>
        internal static string ToBase64Url(ReadOnlySpan<byte> bytes)
            => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }
}
