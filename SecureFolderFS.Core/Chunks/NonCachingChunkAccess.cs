using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.Sdk.Tracking;
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal sealed class NonCachingChunkAccess : BaseChunkAccess
    {
        public NonCachingChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatsTracker? statsTracker)
            : base(chunkReader, chunkWriter, contentCrypt, statsTracker)
        {
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            // Rent buffer
            var cleartextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkCleartextSize);
            var realCleartextChunk = cleartextChunk.AsSpan(0, contentCrypt.ChunkCleartextSize);

            // Read chunk
            var read = chunkReader.ReadChunk(chunkNumber, realCleartextChunk);

            // Check for errors
            if (read < 0)
            {
                ArrayPool<byte>.Shared.Return(cleartextChunk);
                return read;
            }

            // Copy from chunk
            var count = Math.Min(read - offsetInChunk, destination.Length);
            if (count <= 0)
                return 0;

            realCleartextChunk.Slice(offsetInChunk, count).CopyTo(destination);

            // Return buffer
            ArrayPool<byte>.Shared.Return(cleartextChunk);

            return count;
        }

        /// <inheritdoc/>
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
        {
            // Rent buffer
            var cleartextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkCleartextSize);
            var realCleartextChunk = cleartextChunk.AsSpan(0, contentCrypt.ChunkCleartextSize);

            // Read chunk
            var read = chunkReader.ReadChunk(chunkNumber, realCleartextChunk);

            // Check for errors
            if (read < 0)
            {
                ArrayPool<byte>.Shared.Return(cleartextChunk);
                return read;
            }

            // Copy to chunk
            var count = Math.Min(contentCrypt.ChunkCleartextSize - offsetInChunk, source.Length);
            if (count <= 0)
                return 0;

            var destination = realCleartextChunk.Slice(offsetInChunk, count);
            source.Slice(0, count).CopyTo(destination);

            // Write to chunk
            chunkWriter.WriteChunk(chunkNumber, destination);

            // Return buffer
            ArrayPool<byte>.Shared.Return(cleartextChunk);

            return count;
        }

        /// <inheritdoc/>
        public override void SetChunkLength(long chunkNumber, int length)
        {
            var cleartextChunk = new byte[contentCrypt.ChunkCleartextSize];
            var read = chunkReader.ReadChunk(chunkNumber, cleartextChunk);
            if (read == -1)
                throw new CryptographicException();

            var newLengthCleartextChunk = cleartextChunk.AsSpan(0, Math.Min(read, length));

            chunkWriter.WriteChunk(chunkNumber, newLengthCleartextChunk);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }
    }
}
