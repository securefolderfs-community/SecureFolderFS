using System;

namespace SecureFolderFS.Core.Chunks
{
    internal interface ICiphertextChunk
    {
        ReadOnlyMemory<byte> Buffer { get; }
    }
}
