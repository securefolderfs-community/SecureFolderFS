using System;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CiphertextAesGcmChunk : BaseCiphertextChunk
    {
        public const int CHUNK_NONCE_SIZE = 12;

        public const int CHUNK_TAG_SIZE = 16;

        public const int CHUNK_FULL_CIPHERTEXT_SIZE = CHUNK_NONCE_SIZE + CleartextAesGcmChunk.CHUNK_CLEARTEXT_SIZE + CHUNK_TAG_SIZE;

        public CiphertextAesGcmChunk(ReadOnlyMemory<byte> buffer)
            : base(buffer)
        {
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
