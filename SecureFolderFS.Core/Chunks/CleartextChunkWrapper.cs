using System;

namespace SecureFolderFS.Core.Chunks
{
    internal sealed class CleartextChunkWrapper
    {
        private readonly byte[] _buffer;
        private int _actualLength;

        public bool NeedsFlush { get; private set; }

        public CleartextChunkWrapper(byte[] buffer, int actualLength)
        {
            _buffer = buffer;
            _actualLength = actualLength;
        }

        public void CopyTo(Span<byte> destinationBuffer, int offset, ref int positionInBuffer)
        {
            var writeCount = Math.Min(_actualLength - offset, destinationBuffer.Length - positionInBuffer);
            var destination = destinationBuffer.Slice(positionInBuffer);
            _buffer.AsSpan(offset, writeCount).CopyTo(destination);
            positionInBuffer += writeCount;
        }

        public void CopyFrom(ReadOnlySpan<byte> sourceBuffer, int offset, ref int positionInBuffer)
        {
            NeedsFlush = true;

            var readCount = Math.Min(_buffer.Length - offset, sourceBuffer.Length - positionInBuffer);
            var destination = _buffer.AsSpan(offset, readCount);
            sourceBuffer.Slice(positionInBuffer, readCount).CopyTo(destination);
            positionInBuffer += readCount;

            _actualLength = Math.Max(_actualLength, readCount + offset);
        }

        public void SetLength(int length) // TODO: OPTIMIZE - check if this is called at all
        {
            if (_actualLength > length)
            {
                _actualLength = length;
                NeedsFlush = true;
            }
        }

        public static implicit operator ReadOnlySpan<byte>(CleartextChunkWrapper chunkWrapper) => chunkWrapper._buffer.AsSpan(0, chunkWrapper._actualLength);
    }
}
