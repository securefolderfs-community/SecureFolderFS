using System;
using System.IO;

namespace SecureFolderFS.Core.Chunks
{
    internal interface ICleartextChunk : IDisposable
    {
        int ActualLength { get; }

        bool NeedsFlush { get; }

        void CopyTo(MemoryStream destinationStream, int offset);

        int CopyFrom(MemoryStream sourceStream, int offset);

        void CopyFrom2(ReadOnlySpan<byte> sourceBuffer, int offset, ref int positionInBuffer);

        void SetActualLength(int length);

        byte[] ToArray();
    }
}
