using System;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Sdk.Streams;

namespace SecureFolderFS.Core.Streams
{
    internal interface ICleartextFileStreamInternal
    {
        IChunkReceiver ChunkReceiver { get; set; }

        Action<ICleartextFileStream> StreamClosedCallback { get; set; }

        ICiphertextFileStream GetInternalCiphertextFileStream();
    }
}
