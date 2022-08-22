using System;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal interface IChunkWriterDeprecated : IDisposable
    {
        void WriteChunk(long chunkNumber, ICleartextChunk cleartextChunk);
    }
}
