using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    /// <summary>
    /// Represents a decrypting stream used to read/write encrypted data.
    /// </summary>
    public abstract class CleartextStream : Stream
    {
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

        /// <inheritdoc cref="FileStream.Lock"/>
        public abstract void Lock(long position, long length);

        /// <inheritdoc cref="FileStream.Unlock"/>
        public abstract void Unlock(long position, long length);
    }
}
