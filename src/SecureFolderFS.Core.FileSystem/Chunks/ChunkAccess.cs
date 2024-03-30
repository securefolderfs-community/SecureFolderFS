using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Statistics;
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides access to cleartext chunks data in individual files.
    /// </summary>
    internal class ChunkAccess : IDisposable
    {
        protected readonly ChunkReader chunkReader;
        protected readonly ChunkWriter chunkWriter;
        protected readonly IContentCrypt contentCrypt;
        protected readonly IFileSystemStatistics fileSystemStatistics;

        public ChunkAccess(ChunkReader chunkReader, ChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatistics fileSystemStatistics)
        {
            this.chunkReader = chunkReader;
            this.chunkWriter = chunkWriter;
            this.contentCrypt = contentCrypt;
            this.fileSystemStatistics = fileSystemStatistics;
        }

        /// <summary>
        /// Copies bytes from chunk at specified <paramref name="chunkNumber"/> into <paramref name="destination"/>.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to copy from.</param>
        /// <param name="destination">The destination buffer to copy to.</param>
        /// <param name="offsetInChunk">The offset in chunk to start copying from.</param>
        /// <returns>The amount of bytes copied. If successful, value is non-negative.</returns>
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

        /// <summary>
        /// Copies bytes from <paramref name="source"/> into chunk at specified <paramref name="chunkNumber"/>.
        /// </summary>
        /// <param name="chunkNumber">The number of chunk to copy to.</param>
        /// <param name="source">The source buffer to copy from.</param>
        /// <param name="offsetInChunk">The offset in chunk to start copying to.</param>
        /// <returns>The amount of bytes copied. If successful, value is non-negative.</returns>
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

        /// <summary>
        /// Sets the length for specified chunk to <paramref name="length"/>.
        /// </summary>
        /// <param name="chunkNumber">The to chunk to modify at specified chunk number.</param>
        /// <param name="length">The length to extend or truncate to.</param>
        /// <param name="includeCurrentLength">Determines whether to include or exclude existing chunk length when resizing.</param>
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

        /// <summary>
        /// Flushes outstanding chunks to disk.
        /// </summary>
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
