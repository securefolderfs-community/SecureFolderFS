using System;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal abstract class BaseChunkAccess : IChunkAccess
    {
        protected readonly IChunkReader chunkReader;
        protected readonly IChunkWriter chunkWriter;
        protected readonly IContentCrypt contentCrypt;
        protected readonly IFileSystemStatsTracker? statsTracker;

        protected BaseChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatsTracker? statsTracker)
        {
            this.chunkReader = chunkReader;
            this.chunkWriter = chunkWriter;
            this.contentCrypt = contentCrypt;
            this.statsTracker = statsTracker;
        }

        /// <inheritdoc/>
        public abstract int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk);

        /// <inheritdoc/>
        public abstract int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk);

        /// <inheritdoc/>
        public abstract void SetChunkLength(long chunkNumber, int length);

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
