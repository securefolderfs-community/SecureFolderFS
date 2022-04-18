using System;
using System.IO;

namespace SecureFolderFS.Core.Chunks
{
    internal interface ICleartextChunk : IDisposable
    {
        int ActualLength { get; }

        bool NeedsFlush { get; }

        void CopyTo(MemoryStream destinationStream, int offset);

        void CopyFrom(ReadOnlySpan<byte> sourceBuffer, int offset, ref int positionInBuffer);

        void SetActualLength(int length);

        ReadOnlySpan<byte> AsSpan();
    }
}
