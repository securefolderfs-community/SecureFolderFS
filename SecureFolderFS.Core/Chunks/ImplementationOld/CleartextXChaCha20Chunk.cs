namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CleartextXChaCha20Chunk : BaseCleartextChunk
    {
        public const int CHUNK_CLEARTEXT_SIZE = 32 * 1024;

        public CleartextXChaCha20Chunk(byte[] cleartextChunkBuffer, int actualLength)
            : base(cleartextChunkBuffer, actualLength)
        {
        }
    }
}
