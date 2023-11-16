using System;

namespace SecureFolderFS.Shared.Helpers
{
    /// <summary>
    /// Holds a reference to a byte array buffer.
    /// </summary>
    public class BufferHolder
    {
        /// <summary>
        /// Gets the held byte array buffer.
        /// </summary>
        public byte[] Buffer { get; }

        public BufferHolder(byte[] buffer)
        {
            Buffer = buffer;
        }

        public BufferHolder(int bufferLength)
            : this(new byte[bufferLength])
        {
        }

        public static implicit operator Span<byte>(BufferHolder bufferHolder) => bufferHolder.Buffer;
        public static implicit operator ReadOnlySpan<byte>(BufferHolder bufferHolder) => bufferHolder.Buffer;
    }
}
