using System;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CiphertextXChaCha20Chunk : BaseCiphertextChunk
    {
        public const int CHUNK_NONCE_SIZE = 24;

        public const int CHUNK_TAG_SIZE = 16;

        public const int CHUNK_FULL_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CleartextXChaCha20Chunk.CHUNK_CLEARTEXT_SIZE + CHUNK_TAG_SIZE;

        public CiphertextXChaCha20Chunk(ReadOnlyMemory<byte> buffer)
            : base(buffer)
        {
        }

        public ReadOnlySpan<byte> GetPayloadWithTagAsSpan()
        {
            return Buffer.Slice(CHUNK_NONCE_SIZE).Span;
        }

        public override ReadOnlySpan<byte> GetNonceAsSpan()
        {
            return Buffer.Slice(0, CHUNK_NONCE_SIZE).Span;
        }

        public override ReadOnlySpan<byte> GetPayloadAsSpan()
        {
            return Buffer.Slice(CHUNK_NONCE_SIZE, Buffer.Length - (CHUNK_NONCE_SIZE + CHUNK_TAG_SIZE)).Span;
        }

        public override ReadOnlySpan<byte> GetAuthAsSpan()
        {
            return Buffer.Slice(Buffer.Length - CHUNK_TAG_SIZE).Span;
        }
    }
}
