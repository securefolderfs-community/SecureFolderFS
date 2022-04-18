using System;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Sdk.Streams;

namespace SecureFolderFS.Core.Streams
{
    [Obsolete("This interface should not be used.")]
    internal interface ICleartextFileStreamInternal
    {
        IChunkReceiver ChunkReceiver { get; set; }

        Action<ICleartextFileStream> StreamClosedCallback { get; set; }

        [Obsolete("This method should not be used.")]
        internal ICiphertextFileStream DangerousGetInternalCiphertextFileStream();
    }
}
