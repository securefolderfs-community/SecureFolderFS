using System;
using SecureFolderFS.Core.Chunks.IO;

namespace SecureFolderFS.Core.Streams
{
    internal interface ICleartextFileStreamInternal
    {
        IChunkReceiver ChunkReceiver { get; set; }

        Action<ICleartextFileStream> StreamClosedCallback { get; set; }

        ICiphertextFileStream GetInternalCiphertextFileStream();
    }
}
