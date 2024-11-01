using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides access to plaintext chunks data in individual files.
    /// </summary>
    internal class ChunkAccess : IDisposable
    {
        protected readonly ChunkReader chunkReader;
        protected readonly ChunkWriter chunkWriter;
        protected readonly IContentCrypt contentCrypt;
        protected readonly IFileSystemStatistics fileSystemStatistics;

        /// <summary>
        /// Determines whether there are outstanding chunks ready to be flushed to disk.
        /// </summary>
        public virtual bool FlushAvailable { get; } = false;

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
            var PlaintextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkPlaintextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realPlaintextChunk = PlaintextChunk.AsSpan(0, contentCrypt.ChunkPlaintextSize);

                // Read chunk
                var read = chunkReader.ReadChunk(chunkNumber, realPlaintextChunk);

                // Check for any errors
                if (read < 0)
                    return read;

                // Copy from chunk
                var count = Math.Min(read - offsetInChunk, destination.Length);
                if (count <= 0)
                    return 0;

                realPlaintextChunk.Slice(offsetInChunk, count).CopyTo(destination);

                return count;
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(PlaintextChunk);
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
            var PlaintextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkPlaintextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realPlaintextChunk = PlaintextChunk.AsSpan(0, contentCrypt.ChunkPlaintextSize);

                // Read chunk
                var read = chunkReader.ReadChunk(chunkNumber, realPlaintextChunk);

                // Check for any errors
                if (read < 0)
                    return read;

                // Copy to chunk
                var count = Math.Min(contentCrypt.ChunkPlaintextSize - offsetInChunk, source.Length);
                if (count <= 0)
                    return 0;

                var destination = realPlaintextChunk.Slice(offsetInChunk, count);
                source.Slice(0, count).CopyTo(destination);

                // Write to chunk
                chunkWriter.WriteChunk(chunkNumber, destination);

                return count;
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(PlaintextChunk);
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
            var PlaintextChunk = ArrayPool<byte>.Shared.Rent(contentCrypt.ChunkPlaintextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realPlaintextChunk = PlaintextChunk.AsSpan(0, contentCrypt.ChunkPlaintextSize);

                // Read chunk
                var read = chunkReader.ReadChunk(chunkNumber, realPlaintextChunk);

                // Check for any errors
                if (read < 0)
                    throw new CryptographicException();

                // Add read length of existing chunk data to the full length if specified
                length += includeCurrentLength ? read : 0;
                length = Math.Max(length, 0);

                Span<byte> newPlaintextChunk;

                // Determine whether to extend or truncate the chunk
                if (length < read)
                {
                    // Truncate chunk
                    newPlaintextChunk = realPlaintextChunk.Slice(0, Math.Min(read, length));
                }
                else if (read < length)
                {
                    // Clear residual data from ArrayPool and append zeros
                    realPlaintextChunk.Slice(read).Clear();

                    // Extend chunk
                    newPlaintextChunk = realPlaintextChunk.Slice(0, Math.Min(length, contentCrypt.ChunkPlaintextSize));
                }
                else
                    return; // Ignore resizing the same length
                
                // Save newly modified chunk
                chunkWriter.WriteChunk(chunkNumber, newPlaintextChunk);
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(PlaintextChunk);
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
