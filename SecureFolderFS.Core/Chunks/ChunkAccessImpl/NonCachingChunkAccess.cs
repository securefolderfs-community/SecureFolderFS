using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SecureFolderFS.Core.Security.ContentCrypt.FileContent;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkAccess"/>
    internal sealed class NonCachingChunkAccess : BaseChunkAccess
    {
        private readonly IContentCrypt _contentCrypt;

        public NonCachingChunkAccess(IChunkReader chunkReader, IChunkWriter chunkWriter)
            : base(chunkReader, chunkWriter)
        {
        }

        /// <inheritdoc/>
        public override int CopyFromChunk(long chunkNumber, Span<byte> destination, int offsetInChunk)
        {
            Span<byte> chunkData = stackalloc byte[_contentCrypt.ChunkCleartextSize];

            chunkReader.ReadChunk(chunkNumber, destination);
        }

        /// <inheritdoc/>
        public override int CopyToChunk(long chunkNumber, ReadOnlySpan<byte> source, int offsetInChunk)
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
