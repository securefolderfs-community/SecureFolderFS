using System;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal abstract class BaseChunkAccess : IChunkAccess
    {
        protected readonly IContentCrypt contentCrypt;
        protected readonly IChunkReader chunkReader;
        protected readonly IChunkWriter chunkWriter;

        protected BaseChunkAccess(IContentCrypt contentCrypt, IChunkReader chunkReader, IChunkWriter chunkWriter)
        {
            this.contentCrypt = contentCrypt;
            this.chunkReader = chunkReader;
            this.chunkWriter = chunkWriter;
        }

        /// <inheritdoc/>
        public abstract int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk);

        /// <inheritdoc/>
        public abstract int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk);

        /// <inheritdoc/>
        public abstract void Flush();

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            chunkReader.Dispose();
            chunkWriter.Dispose();
        }
    }
}
