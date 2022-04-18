using System;
using System.IO;
using SecureFolderFS.Core.Extensions;

namespace SecureFolderFS.Core.Chunks.Implementation
{
    internal abstract class BaseCleartextChunk : ICleartextChunk
    {
        protected readonly byte[] buffer;

        private bool _disposed;

        public bool NeedsFlush { get; private set; }

        public int ActualLength { get; private set; }

        protected BaseCleartextChunk(byte[] cleartextChunkBuffer, int actualLength)
        {
            this.buffer = cleartextChunkBuffer;
            this.ActualLength = actualLength;
        }

        public virtual void CopyTo(MemoryStream destinationStream, int offset)
        {
            AssertNotDisposed();

            var writeCount = Math.Min(ActualLength - offset, (int)destinationStream.RemainingLength());
            destinationStream.Write(buffer, offset, writeCount);
        }

        public virtual void CopyFrom(ReadOnlySpan<byte> sourceBuffer, int offset, ref int positionInBuffer)
        {
            AssertNotDisposed();

            NeedsFlush = true;

            var readCount = Math.Min(buffer.Length - offset, sourceBuffer.Length - positionInBuffer);
            var destination = buffer.AsSpan(offset, readCount);
            sourceBuffer.Slice(0, readCount).CopyTo(destination);
            positionInBuffer += readCount;

            ActualLength = Math.Max(ActualLength, readCount + offset);
        }

        public virtual void SetActualLength(int length)
        {
            AssertNotDisposed();

            if (ActualLength > length)
            {
                ActualLength = length;
                NeedsFlush = true;
            }
        }

        public virtual ReadOnlySpan<byte> AsSpan()
        {
            AssertNotDisposed();

            return buffer.AsSpan(0, ActualLength);
        }

        protected void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public virtual void Dispose()
        {
            _disposed = true;
            Array.Clear(buffer);
        }
    }
}
