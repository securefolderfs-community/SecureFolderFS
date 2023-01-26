using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Cryptography.Constants.Crypt.Headers.XChaCha20Poly1305;

namespace SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions
{
    internal static class XChaChaHeaderExtensions
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
            return header.Slice(0, HEADER_TAG_SIZE);
        }
    }
}
