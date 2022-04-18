using SecureFolderFS.Core.Chunks.Implementation;
using System;

namespace SecureFolderFS.Core.Chunks.Factory
{
    internal sealed class AesGcmChunkFactory : IChunkFactory
    {
        public ICleartextChunk FromCleartextChunkBuffer(byte[] cleartextChunkBuffer)
        {
            return FromCleartextChunkBuffer(cleartextChunkBuffer, cleartextChunkBuffer.Length);
        }

        public ICleartextChunk FromCleartextChunkBuffer(byte[] cleartextChunkBuffer, int actualLength)
        {
            return new CleartextAesGcmChunk(cleartextChunkBuffer, actualLength);
        }

        public ICiphertextChunk FromCiphertextChunkBuffer(ReadOnlyMemory<byte> ciphertextChunkBuffer)
        {
            return new CiphertextAesGcmChunk(ciphertextChunkBuffer);
        }
    }
}
