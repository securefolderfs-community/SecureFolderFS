using System;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal interface IChunkReader : IDisposable
    {
        ICleartextChunk ReadChunk(long chunkNumber);
    }
}
