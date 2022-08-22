using System;

namespace SecureFolderFS.Core.Chunks
{
    internal interface ICleartextChunk
    {
        int ActualLength { get; }

        bool NeedsFlush { get; }

        void CopyTo(Span<byte> destinationBuffer, int offset, ref int positionInBuffer);

        void CopyFrom(ReadOnlySpan<byte> sourceBuffer, int offset, ref int positionInBuffer);

        void SetActualLength(int length);

        ReadOnlySpan<byte> AsSpan();
    }
}
