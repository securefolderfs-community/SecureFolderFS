using System;

namespace SecureFolderFS.Core.BufferHolders
{
    internal abstract class BaseBufferHolder
    {
        protected readonly byte[] buffer;

        public BaseBufferHolder(byte[] buffer)
        {
            this.buffer = buffer;
        }

        public void Clear()
        {
            Array.Clear(buffer);
        }

        public static implicit operator Span<byte>(BaseBufferHolder bufferHolder) => bufferHolder.buffer;

        public static implicit operator ReadOnlySpan<byte>(BaseBufferHolder bufferHolder) => bufferHolder.buffer;
    }
}
