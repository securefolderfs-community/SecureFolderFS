using SecureFolderFS.Core.Cryptography.ContentCrypt;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Chunks;
using System;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal abstract class BaseChunkAccess : IChunkAccess
    {
        protected readonly IChunkReader chunkReader;
        protected readonly IChunkWriter chunkWriter;
        protected readonly IContentCrypt contentCrypt;
        protected readonly IFileSystemStatistics? fileSystemStatistics;

        protected BaseChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter, IContentCrypt contentCrypt, IFileSystemStatistics? fileSystemStatistics)
        {
            this.chunkReader = chunkReader;
            this.chunkWriter = chunkWriter;
            this.contentCrypt = contentCrypt;
            this.fileSystemStatistics = fileSystemStatistics;
        }

        /// <inheritdoc/>
        public abstract int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk);

        /// <inheritdoc/>
        public abstract int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk);

        /// <inheritdoc/>
        public abstract void SetChunkLength(long chunkNumber, int length, bool includeReadLength = false);

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
