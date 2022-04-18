using System;

namespace SecureFolderFS.Core.Chunks
{
    internal interface IChunkFactory
    {
        ICleartextChunk FromCleartextChunkBuffer(byte[] cleartextChunkBuffer);

        ICleartextChunk FromCleartextChunkBuffer(byte[] cleartextChunkBuffer, int actualLength);

        ICiphertextChunk FromCiphertextChunkBuffer(ReadOnlyMemory<byte> ciphertextChunkBuffer);
    }
}
