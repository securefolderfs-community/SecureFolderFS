using System;

namespace SecureFolderFS.Shared.Helpers
{
    public abstract class BufferHolder
    {
        public byte[] Buffer { get; }

        public virtual int Length => Buffer.Length;

        protected BufferHolder(byte[] buffer)
        {
            Buffer = buffer;
        }

        protected BufferHolder(int bufferLength)
            : this(new byte[bufferLength])
        {
        }

        public virtual void Clear()
        {
            Array.Clear(Buffer);
        }

        public static implicit operator Span<byte>(BufferHolder bufferHolder) => bufferHolder.Buffer;
        public static implicit operator ReadOnlySpan<byte>(BufferHolder bufferHolder) => bufferHolder.Buffer;
    }
}
