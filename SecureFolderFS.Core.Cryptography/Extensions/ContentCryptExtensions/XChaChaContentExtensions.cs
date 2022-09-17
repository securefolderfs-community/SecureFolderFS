using System;
using System.Runtime.CompilerServices;
using static SecureFolderFS.Core.Cryptography.Constants.Security.Chunks.XChaCha20Poly1305;

namespace SecureFolderFS.Core.Cryptography.Extensions.ContentCryptExtensions
{
    internal static class XChaChaContentExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetChunkNonce(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<byte> GetChunkPayloadWithTag(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(CHUNK_NONCE_SIZE);
        }
    }
}
