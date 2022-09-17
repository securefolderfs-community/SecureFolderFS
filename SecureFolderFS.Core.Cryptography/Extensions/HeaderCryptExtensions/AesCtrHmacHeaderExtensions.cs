using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Cryptography.Constants.Security.Headers.AesCtrHmac;

namespace SecureFolderFS.Core.Cryptography.Extensions.HeaderCryptExtensions
{
    internal static class AesCtrHmacHeaderExtensions
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
        public static ReadOnlySpan<byte> GetHeaderMac(this ReadOnlySpan<byte> ciphertextHeader)
        {
            return ciphertextHeader.Slice(ciphertextHeader.Length - HEADER_MAC_SIZE);
        }
    }
}
