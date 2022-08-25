using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using System;
using SecureFolderFS.Core.Exceptions;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal sealed class NonCachingChunkAccess : BaseChunkAccess
    {
        public NonCachingChunkAccess(IContentCrypt contentCrypt, IChunkReader chunkReader, IChunkWriter chunkWriter)
            : base(contentCrypt, chunkReader, chunkWriter)
        {
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            var cleartextChunk = new byte[contentCrypt.ChunkCleartextSize];
            var read = chunkReader.ReadChunk(chunkNumber, cleartextChunk);
            if (read == -1)
                throw new UnauthenticChunkException();

            var count = Math.Min(read - offsetInChunk, destination.Length);
            cleartextChunk.AsSpan(offsetInChunk, count).CopyTo(destination);

            return count;
        }

        /// <inheritdoc/>
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
        {
            var cleartextChunk = new byte[contentCrypt.ChunkCleartextSize];
            var read = chunkReader.ReadChunk(chunkNumber, cleartextChunk);
            if (read == -1)
                throw new UnauthenticChunkException();

            var count = Math.Min(contentCrypt.ChunkCleartextSize - offsetInChunk, source.Length);
            var destination = cleartextChunk.AsSpan(offsetInChunk, count);
            source.Slice(0, count).CopyTo(destination);

            // ActualLength = Math.Max(ActualLength, count + offsetInChunk) doesn't matter here I think
            
            // Write the chunk
            chunkWriter.WriteChunk(chunkNumber, destination);

            return count;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }
    }
}
