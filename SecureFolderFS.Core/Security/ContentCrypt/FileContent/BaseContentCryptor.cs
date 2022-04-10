using System;
using System.Security.Cryptography;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Security.KeyCrypt;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    internal abstract class BaseContentCryptor<TFileHeader, TCleartextChunk, TCiphertextChunk> : IFileContentCryptor
        where TFileHeader : class, IFileHeader
        where TCleartextChunk : class, ICleartextChunk
        where TCiphertextChunk : class, ICiphertextChunk
    {
        protected readonly IKeyCryptor keyCryptor;

        protected readonly IChunkFactory chunkFactory;

        protected readonly RandomNumberGenerator secureRandom;

        private bool _disposed;

        public abstract int ChunkCleartextSize { get; }

        public abstract int ChunkFullCiphertextSize { get; }

        protected BaseContentCryptor(IKeyCryptor keyCryptor, IChunkFactory chunkFactory)
        {
            this.keyCryptor = keyCryptor;
            this.chunkFactory = chunkFactory;
            this.secureRandom = RandomNumberGenerator.Create();
        }

        public ICiphertextChunk EncryptChunk(ICleartextChunk cleartextChunk, long chunkNumber, IFileHeader fileHeader)
        {
            AssertNotDisposed();

            if (fileHeader is not TFileHeader requestedFileHeader)
            {
                throw ErrorHandlingHelpers.GetBadTypeException(nameof(fileHeader), typeof(TFileHeader));
            }
            if (cleartextChunk is not TCleartextChunk requestedCleartextChunk)
            {
                throw ErrorHandlingHelpers.GetBadTypeException(nameof(cleartextChunk), typeof(TCleartextChunk));
            }

            return EncryptChunk(requestedCleartextChunk, chunkNumber, requestedFileHeader);
        }

        public ICleartextChunk DecryptChunk(ICiphertextChunk ciphertextChunk, long chunkNumber, IFileHeader fileHeader, bool checkIntegrity)
        {
            AssertNotDisposed();

            if (fileHeader is not TFileHeader requestedFileHeader)
            {
                throw ErrorHandlingHelpers.GetBadTypeException(nameof(fileHeader), typeof(TFileHeader));
            }
            if (ciphertextChunk is not TCiphertextChunk requestedCiphertextChunk)
            {
                throw ErrorHandlingHelpers.GetBadTypeException(nameof(ciphertextChunk), typeof(TCiphertextChunk));
            }

            // Checks chunk integrity or throws appropriate error
            CheckIntegrity(requestedCiphertextChunk, requestedFileHeader, chunkNumber, checkIntegrity);

            return DecryptChunk(requestedCiphertextChunk, chunkNumber, requestedFileHeader);
        }

        public long CalculateCiphertextSize(long cleartextSize)
        {
            AssertNotDisposed();

            long overheadPerChunk = ChunkFullCiphertextSize - ChunkCleartextSize;
            long fullChunksCount = cleartextSize / ChunkCleartextSize;
            long additionalCleartextBytes = cleartextSize % ChunkCleartextSize;
            long additionalCiphertextBytes = (additionalCleartextBytes == 0L) ? 0L : additionalCleartextBytes + overheadPerChunk;
            return ChunkFullCiphertextSize * fullChunksCount + additionalCiphertextBytes;
        }

        public long CalculateCleartextSize(long ciphertextSize)
        {
            AssertNotDisposed();

            long chunkOverhead = ChunkFullCiphertextSize - ChunkCleartextSize;
            long chunksCount = ciphertextSize / ChunkFullCiphertextSize;
            long additionalCiphertextBytes = ciphertextSize % ChunkFullCiphertextSize;

            if (additionalCiphertextBytes > 0 && additionalCiphertextBytes <= chunkOverhead)
            {
                throw new ArgumentException("Bad chunk ciphertext size.");
            }

            long additionalCleartextBytes = (additionalCiphertextBytes == 0L) ? 0L : additionalCiphertextBytes - chunkOverhead;
            long final = ChunkCleartextSize * chunksCount + additionalCleartextBytes;
            return final > 0L ? final : 0L;
        }

        protected byte[] ExtendCleartextChunkBuffer(byte[] cleartextChunk)
        {
            AssertNotDisposed();

            if (cleartextChunk.Length != ChunkCleartextSize)
            {
                return cleartextChunk.ArrayWithSize(ChunkCleartextSize);
            }

            return cleartextChunk;
        }

        protected abstract ICiphertextChunk EncryptChunk(TCleartextChunk cleartextChunk, long chunkNumber, TFileHeader fileHeader);

        protected abstract ICleartextChunk DecryptChunk(TCiphertextChunk ciphertextChunk, long chunkNumber, TFileHeader fileHeader);

        protected abstract void CheckIntegrity(TCiphertextChunk ciphertextChunk, TFileHeader fileHeader, long chunkNumber, bool requestedIntegrityCheck);

        protected void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public virtual void Dispose()
        {
            _disposed = true;
            secureRandom.Dispose();
        }
    }
}
