using System;
using System.IO;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Ftp
{
    internal sealed class SeekableFtpStream : Stream, IWrapper<Stream>
    {
        private long _length;
        private long _position;

        /// <inheritdoc/>
        public override bool CanRead => Inner.CanRead;

        /// <inheritdoc/>
        public override bool CanWrite => Inner.CanWrite;

        /// <inheritdoc/>
        public override bool CanSeek { get; }

        /// <inheritdoc/>
        public override long Length => _length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc/>
        public Stream Inner { get; }

        public SeekableFtpStream(Stream stream, long length)
        {
            Inner = stream;
            CanSeek = length >= 0L;
            _length = length;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException("Stream is not readable.");

            var bytesRead = Inner.Read(buffer, offset, count);
            _position += bytesRead;
            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException("Stream is not writable.");

            Inner.Write(buffer, offset, count);
            _position += count;
            _length = Math.Max(_length, _position);
        }

        public override void Flush()
        {
            Inner.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };

            if (newPos < 0 || (!CanWrite && newPos > _length))
                throw new ArgumentOutOfRangeException(nameof(offset));

            return _position = newPos;
        }

        public override void SetLength(long value)
        {
            if (!CanWrite)
                throw new NotSupportedException("SetLength is only supported in write mode.");

            _length = value;
            _position = Math.Min(_position, _length);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Inner.Dispose();

            base.Dispose(disposing);
        }
    }
}
