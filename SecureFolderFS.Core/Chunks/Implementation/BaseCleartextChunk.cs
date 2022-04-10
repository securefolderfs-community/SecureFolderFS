﻿using System;
using System.IO;
using SecureFolderFS.Core.Extensions;
using SecureFolderFS.Shared.Extensions;

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

        public virtual void CopyFrom(MemoryStream sourceStream, int offset)
        {
            AssertNotDisposed();

            NeedsFlush = true;

            var readCount = Math.Min(buffer.Length - offset, (int)sourceStream.RemainingLength());
            sourceStream.Read(buffer, offset, readCount);
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

        public virtual byte[] ToArray()
        {
            AssertNotDisposed();

            byte[] returnBuffer = new byte[ActualLength];
            Array.Copy(buffer, 0, returnBuffer, 0, ActualLength);

            return returnBuffer;
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
