using SecureFolderFS.Core.Chunks.Implementation;

namespace SecureFolderFS.Core.Chunks.Factory
{
    internal sealed class XChaCha20ChunkFactory : IChunkFactory
    {
        public ICleartextChunk FromCleartextChunkBuffer(byte[] cleartextChunkBuffer)
        {
            return FromCleartextChunkBuffer(cleartextChunkBuffer, cleartextChunkBuffer.Length);
        }

        public ICleartextChunk FromCleartextChunkBuffer(byte[] cleartextChunkBuffer, int actualLength)
        {
            return new CleartextXChaCha20Chunk(cleartextChunkBuffer, actualLength);
        }

        public ICiphertextChunk FromCiphertextChunkBuffer(byte[] ciphertextChunkBuffer)
        {
            return CiphertextXChaCha20Chunk.FromCiphertextChunkBuffer(ciphertextChunkBuffer);
        }
    }
}
