using System;
using static SecureFolderFS.Core.Constants.Security.Headers.AesGcm;

namespace SecureFolderFS.Core.Extensions.SecurityExtensions.Header
{
    internal static class AesGcmHeaderExtensions
    {
        public static ReadOnlySpan<byte> GetHeaderNonce(this ReadOnlySpan<byte> header)
        {
            return header.Slice(0, HEADER_NONCE_SIZE);
        }

        public static ReadOnlySpan<byte> GetHeaderContentKey(this ReadOnlySpan<byte> header)
        {
            return header.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE);
        }

        public static ReadOnlySpan<byte> GetHeaderTag(this ReadOnlySpan<byte> header)
        {
            return header.Slice(0, HEADER_TAG_SIZE);
        }
    }
}
