using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Statistics;
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal class InstantChunkAccess : IChunkAccess
    {
        protected readonly IChunkReader chunkReader;
        protected readonly IChunkWriter chunkWriter;
        protected readonly IContentCrypt contentCrypt;
        protected readonly IFileSystemStatistics? fileSystemStatistics;

        public InstantChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatistics? fileSystemStatistics)
        {
            this.chunkReader = chunkReader;
            this.chunkWriter = chunkWriter;
            this.contentCrypt = contentCrypt;
            this.fileSystemStatistics = fileSystemStatistics;
        }

        /// <inheritdoc/>
        public virtual int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            // Rent buffer
            var cleartextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkCleartextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realCleartextChunk = cleartextChunk.AsSpan(0, contentCrypt.ChunkCleartextSize);

                // Read chunk
                var read = chunkReader.ReadChunk(chunkNumber, realCleartextChunk);

                // Check for any errors
                if (read < 0)
                    return read;

                // Copy from chunk
                var count = Math.Min(read - offsetInChunk, destination.Length);
                if (count <= 0)
                    return 0;

                realCleartextChunk.Slice(offsetInChunk, count).CopyTo(destination);

                return count;
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(cleartextChunk);
            }
        }

        /// <inheritdoc/>
        public virtual int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
        {
            // Rent buffer
            var cleartextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkCleartextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realCleartextChunk = cleartextChunk.AsSpan(0, contentCrypt.ChunkCleartextSize);

                // Read chunk
                var read = chunkReader.ReadChunk(chunkNumber, realCleartextChunk);

                // Check for any errors
                if (read < 0)
                    return read;

                // Copy to chunk
                var count = Math.Min(contentCrypt.ChunkCleartextSize - offsetInChunk, source.Length);
                if (count <= 0)
                    return 0;

                var destination = realCleartextChunk.Slice(offsetInChunk, count);
                source.Slice(0, count).CopyTo(destination);

                // Write to chunk
                chunkWriter.WriteChunk(chunkNumber, destination);

                return count;
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(cleartextChunk);
            }
        }

        /// <inheritdoc/>
        public virtual void SetChunkLength(long chunkNumber, int length, bool includeCurrentLength = false)
        {
            // Rent buffer
            var cleartextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkCleartextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realCleartextChunk = cleartextChunk.AsSpan(0, contentCrypt.ChunkCleartextSize);

                // Read chunk
                var read = chunkReader.ReadChunk(chunkNumber, realCleartextChunk);

                // Check for any errors
                if (read < 0)
                    throw new CryptographicException();

                // Add read length of existing chunk data to the full length if specified
                length += includeCurrentLength ? read : 0;
                length = Math.Max(length, 0);

                Span<byte> newCleartextChunk;

                // Determine whether to extend or truncate the chunk
                if (length < read)
                {
                    // Truncate chunk
                    newCleartextChunk = realCleartextChunk.Slice(0, Math.Min(read, length));
                }
                else if (read < length)
                {
                    // Clear residual data from ArrayPool and append zeros
                    realCleartextChunk.Slice(read).Clear();

                    // Extend chunk
                    newCleartextChunk = realCleartextChunk.Slice(0, Math.Min(length, contentCrypt.ChunkCleartextSize));
                }
                else
                    return; // Ignore resizing the same length
                
                // Save newly modified chunk
                chunkWriter.WriteChunk(chunkNumber, newCleartextChunk);
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(cleartextChunk);
            }
        }

        /// <inheritdoc/>
        public virtual void Flush()
        {
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            chunkReader.Dispose();
            chunkWriter.Dispose();
        }
    }
}
