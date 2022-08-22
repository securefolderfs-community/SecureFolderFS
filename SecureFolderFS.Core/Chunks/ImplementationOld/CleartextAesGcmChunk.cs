namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal sealed class CleartextAesGcmChunk : BaseCleartextChunk
    {
        public const int CHUNK_CLEARTEXT_SIZE = 32 * 1024;

        public CleartextAesGcmChunk(byte[] cleartextChunkBuffer, int actualLength)
            : base(cleartextChunkBuffer, actualLength)
        {
        }
    }
}
