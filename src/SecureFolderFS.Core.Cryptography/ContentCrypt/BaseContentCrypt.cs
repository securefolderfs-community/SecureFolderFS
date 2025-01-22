using System;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal abstract class BaseContentCrypt : IContentCrypt
    {
        protected readonly RandomNumberGenerator secureRandom;

        /// <inheritdoc/>
        public abstract int ChunkPlaintextSize { get; }

        /// <inheritdoc/>
        public abstract int ChunkCiphertextSize { get; }

        /// <inheritdoc/>
        public abstract int ChunkFirstReservedSize { get; }

        protected BaseContentCrypt()
        {
            secureRandom = RandomNumberGenerator.Create();
        }

        /// <inheritdoc/>
        public abstract void EncryptChunk(ReadOnlySpan<byte> plaintextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk);

        /// <inheritdoc/>
        public abstract bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> plaintextChunk);

        /// <inheritdoc/>
        public virtual long CalculateCiphertextSize(long plaintextSize)
        {
            var overheadPerChunk = ChunkCiphertextSize - ChunkPlaintextSize;
            var fullChunksCount = plaintextSize / ChunkPlaintextSize;
            var additionalPlaintextBytes = plaintextSize % ChunkPlaintextSize;
            var additionalCiphertextBytes = (additionalPlaintextBytes == 0L) ? 0L : additionalPlaintextBytes + overheadPerChunk;

            return ChunkCiphertextSize * fullChunksCount + additionalCiphertextBytes;
        }

        /// <inheritdoc/>
        public virtual long CalculatePlaintextSize(long ciphertextSize)
        {
            if (ciphertextSize == 0L)
                return 0L;

            var chunkOverhead = ChunkCiphertextSize - ChunkPlaintextSize;
            var chunksCount = ciphertextSize / ChunkCiphertextSize;
            var additionalCiphertextBytes = ciphertextSize % ChunkCiphertextSize;

            if (additionalCiphertextBytes > 0 && additionalCiphertextBytes <= chunkOverhead)
                return -1L;

            var additionalPlaintextBytes = (additionalCiphertextBytes == 0L) ? 0L : additionalCiphertextBytes - chunkOverhead;
            var final = ChunkPlaintextSize * chunksCount + additionalPlaintextBytes;

            return final >= 0L ? final : -1L;
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            secureRandom.Dispose();
        }
    }
}
