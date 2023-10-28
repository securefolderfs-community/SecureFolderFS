using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Chunks.AesCtrHmac;

namespace SecureFolderFS.Core.Cryptography.Extensions.ContentCryptExtensions
{
    internal static class AesCtrHmacContentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetChunkNonce(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetChunkPayload(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(CHUNK_NONCE_SIZE, ciphertextChunk.Length - (CHUNK_NONCE_SIZE + CHUNK_MAC_SIZE));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetChunkMac(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(ciphertextChunk.Length - CHUNK_MAC_SIZE);
        }
    }
}
