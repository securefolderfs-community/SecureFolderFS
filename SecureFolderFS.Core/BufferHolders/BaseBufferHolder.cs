using System;

namespace SecureFolderFS.Core.BufferHolders
{
    internal abstract class BaseBufferHolder
    {
        protected readonly byte[] buffer;

        public int Length => buffer.Length;

        protected BaseBufferHolder(byte[] buffer)
        {
            this.buffer = buffer;
        }

        protected BaseBufferHolder(int bufferLength)
            : this(new byte[bufferLength])
        {
        }

        public void Clear()
        {
            Array.Clear(buffer);
        }

        public static implicit operator Span<byte>(BaseBufferHolder bufferHolder) => bufferHolder.buffer;

        public static implicit operator ReadOnlySpan<byte>(BaseBufferHolder bufferHolder) => bufferHolder.buffer;
    }
}
