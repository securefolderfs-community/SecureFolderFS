using System;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal interface IChunkWriter : IDisposable
    {
        void WriteChunk(long chunkNumber, ICleartextChunk cleartextChunk);
    }
}
