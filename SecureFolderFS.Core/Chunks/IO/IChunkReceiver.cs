using System;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal interface IChunkReceiver : IDisposable
    {
        ICleartextChunk GetChunk(long chunkNumber);

        void SetChunk(long chunkNumber, ICleartextChunk cleartextChunk);

        void Flush();
    }
}
