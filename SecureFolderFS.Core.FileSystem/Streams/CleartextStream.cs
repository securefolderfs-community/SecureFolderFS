using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Utils;

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
        public sealed override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        /// <inheritdoc/>
        public sealed override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
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
