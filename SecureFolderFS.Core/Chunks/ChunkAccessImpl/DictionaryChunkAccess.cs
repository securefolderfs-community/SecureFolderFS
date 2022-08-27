using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;
using System;
using System.Collections.Concurrent;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal sealed class DictionaryChunkAccess : BaseChunkAccess
    {
        private readonly ConcurrentDictionary<long, CleartextChunkBuffer> _chunkCache;

        public DictionaryChunkAccess(IContentCrypt contentCrypt, IChunkReader chunkReader, IChunkWriter chunkWriter)
            : base(contentCrypt, chunkReader, chunkWriter)
        {
            _chunkCache = new(3, Constants.IO.MAX_CACHED_CHUNKS);
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public override void SetChunkLength(long chunkNumber, int length)
        {
            throw new NotImplementedException();
        }
        
        /// <inheritdoc/>
        public override void Flush()
        {
            throw new NotImplementedException();
        }
    }
}
