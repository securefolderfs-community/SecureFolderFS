using System;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using SecureFolderFS.Core.Security.Cipher;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileContent
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal abstract class BaseContentCrypt : IContentCrypt
    {
        protected readonly ICipherProvider cipherProvider;
        protected readonly RandomNumberGenerator secureRandom;

        /// <inheritdoc/>
        public abstract int ChunkCleartextSize { get; }

        /// <inheritdoc/>
        public abstract int ChunkCiphertextSize { get; }

        /// <inheritdoc/>
        public virtual int ChunkCiphertextOverheadSize => ChunkCiphertextSize - ChunkCleartextSize;

        protected BaseContentCrypt(ICipherProvider cipherProvider)
        {
            this.cipherProvider = cipherProvider;
            this.secureRandom = RandomNumberGenerator.Create();
        }

        /// <inheritdoc/>
        public abstract void EncryptChunk(ReadOnlySpan<byte> cleartextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk);

        /// <inheritdoc/>
        public abstract bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> cleartextChunk);

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual long CalculateCiphertextSize(long cleartextSize)
        {
            var overheadPerChunk = ChunkCiphertextSize - ChunkCleartextSize;
            var fullChunksCount = cleartextSize / ChunkCleartextSize;
            var additionalCleartextBytes = cleartextSize % ChunkCleartextSize;
            var additionalCiphertextBytes = (additionalCleartextBytes == 0L) ? 0L : additionalCleartextBytes + overheadPerChunk;

            return ChunkCiphertextSize * fullChunksCount + additionalCiphertextBytes;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public virtual long CalculateCleartextSize(long ciphertextSize)
        {
            var chunkOverhead = ChunkCiphertextSize - ChunkCleartextSize;
            var chunksCount = ciphertextSize / ChunkCiphertextSize;
            var additionalCiphertextBytes = ciphertextSize % ChunkCiphertextSize;

            if (additionalCiphertextBytes > 0 && additionalCiphertextBytes <= chunkOverhead)
            {
                return -1L;
            }

            var additionalCleartextBytes = (additionalCiphertextBytes == 0L) ? 0L : additionalCiphertextBytes - chunkOverhead;
            var final = ChunkCleartextSize * chunksCount + additionalCleartextBytes;

            return final >= 0L ? final : -1L;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            secureRandom.Dispose();
        }
    }
}
