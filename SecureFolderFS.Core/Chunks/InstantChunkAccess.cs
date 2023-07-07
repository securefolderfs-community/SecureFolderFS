using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Core.FileSystem.Chunks;
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal sealed class InstantChunkAccess : BaseChunkAccess
    {
        public InstantChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatistics? fileSystemStatistics)
            : base(chunkReader, chunkWriter, contentCrypt, fileSystemStatistics)
        {
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
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
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
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
        public override void SetChunkLength(long chunkNumber, int length, bool includeCurrentLength = false)
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
        public override void Flush()
        {
        }
    }
}
