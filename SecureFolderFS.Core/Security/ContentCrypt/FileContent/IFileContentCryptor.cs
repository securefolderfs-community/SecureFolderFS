using System;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.FileHeaders;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    internal interface IFileContentCryptor : IDisposable
    {
        int ChunkCleartextSize { get; }

        int ChunkFullCiphertextSize { get; }

        ICiphertextChunk EncryptChunk(ICleartextChunk cleartextChunk, long chunkNumber, IFileHeader fileHeader);

        ICleartextChunk DecryptChunk(ICiphertextChunk ciphertextChunk, long chunkNumber, IFileHeader fileHeader, bool checkIntegrity);

        long CalculateCiphertextSize(long cleartextSize);

        long CalculateCleartextSize(long ciphertextSize);
    }
}
