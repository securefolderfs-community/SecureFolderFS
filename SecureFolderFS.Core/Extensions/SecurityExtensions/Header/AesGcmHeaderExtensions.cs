using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Constants.Security.Headers.AesGcm;

namespace SecureFolderFS.Core.Extensions.SecurityExtensions.Header
{
    internal static class AesGcmHeaderExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetHeaderNonce(this ReadOnlySpan<byte> header)
        {
            return header.Slice(0, HEADER_NONCE_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetHeaderContentKey(this ReadOnlySpan<byte> header)
        {
            return header.Slice(HEADER_NONCE_SIZE, HEADER_CONTENTKEY_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetHeaderTag(this ReadOnlySpan<byte> header)
        {
            return header.Slice(header.Length - HEADER_TAG_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Span<byte> GetHeaderTag(this Span<byte> header)
        {
            return header.Slice(header.Length - HEADER_TAG_SIZE);
        }
    }
}
