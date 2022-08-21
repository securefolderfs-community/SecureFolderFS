using System;
using static SecureFolderFS.Core.Constants.Security.Chunks.XChaCha20Poly1305;

namespace SecureFolderFS.Core.Extensions.SecurityExtensions.Content
{
    internal static class XChaChaContentExtensions
    {
        public static ReadOnlySpan<byte> GetChunkNonce(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(0, CHUNK_NONCE_SIZE);
        }

        public static ReadOnlySpan<byte> GetChunkPayloadWithTag(this ReadOnlySpan<byte> ciphertextChunk)
        {
            return ciphertextChunk.Slice(CHUNK_NONCE_SIZE);
        }
    }
}
