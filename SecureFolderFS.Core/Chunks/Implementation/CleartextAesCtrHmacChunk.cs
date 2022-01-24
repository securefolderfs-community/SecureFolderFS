namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CleartextAesCtrHmacChunk : BaseCleartextChunk
    {
        public const int CHUNK_CLEARTEXT_SIZE = 32 * 1024;

        public CleartextAesCtrHmacChunk(byte[] cleartextChunkBuffer, int actualLength)
            : base(cleartextChunkBuffer, actualLength)
        {
        }
    }
}
