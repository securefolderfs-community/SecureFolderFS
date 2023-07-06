using SecureFolderFS.Shared.Utils;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    /// <summary>
    /// Represents a decrypting stream used to read/write encrypted data.
    /// </summary>
    public abstract class CleartextStream : Stream, IWrapper<Stream>
    {
        /// <summary>
        /// Gets the underlying ciphertext stream.
        /// </summary>
        public Stream Inner { get; }

        /// <inheritdoc/>
        public override long Length => Inner.Length;

        /// <inheritdoc/>
        public override bool CanRead => Inner.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => Inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => Inner.CanWrite;

        protected CleartextStream(Stream innerStream)
        {
            Inner = innerStream;
        }

        /// <inheritdoc/>
        public sealed override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        public sealed override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        public sealed override int ReadByte()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public sealed override void WriteByte(byte value)
        {
            throw new NotSupportedException();
        }
    }
}
