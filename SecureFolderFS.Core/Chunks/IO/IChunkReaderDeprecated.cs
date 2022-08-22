using System;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal interface IChunkReaderDeprecated : IDisposable
    {
        ICleartextChunk ReadChunk(long chunkNumber);
    }
}
