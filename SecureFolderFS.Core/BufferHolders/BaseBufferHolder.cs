using System;

namespace SecureFolderFS.Core.BufferHolders
{
    internal abstract class BaseBufferHolder
    {
        public byte[] Buffer { get; }

        public int Length => Buffer.Length;

        protected BaseBufferHolder(byte[] buffer)
        {
            this.Buffer = buffer;
        }

        protected BaseBufferHolder(int bufferLength)
            : this(new byte[bufferLength])
        {
        }

        public void Clear()
        {
            Array.Clear(Buffer);
        }

        public static implicit operator Span<byte>(BaseBufferHolder bufferHolder) => bufferHolder.Buffer;

        public static implicit operator ReadOnlySpan<byte>(BaseBufferHolder bufferHolder) => bufferHolder.Buffer;
    }
}
