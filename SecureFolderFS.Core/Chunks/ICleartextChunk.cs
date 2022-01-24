using System;
using System.IO;

namespace SecureFolderFS.Core.Chunks
{
    internal interface ICleartextChunk : IDisposable
    {
        int ActualLength { get; }

        bool NeedsFlush { get; }

        void CopyTo(MemoryStream destinationStream, int offset);

        void CopyFrom(MemoryStream sourceStream, int offset);

        void SetActualLength(int length);

        byte[] ToArray();
    }
}
