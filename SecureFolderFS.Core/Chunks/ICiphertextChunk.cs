using System;

namespace SecureFolderFS.Core.Chunks
{
    internal interface ICiphertextChunk : IDisposable
    {
        byte[] ToArray();
    }
}
