using System;
using System.IO;
using FluentFTP;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.Ftp
{
    /// <summary>
    /// Provides support for the <see cref="Stream.Length"/> property on an FTP stream.
    /// </summary>
    /// <remarks>
    /// The implementation of FluentFTP's <see cref="FtpDataStream"/> does not support the <see cref="Stream.Length"/> property
    /// upon opening an existing file or writing contents to the newly created file.
    /// </remarks>
    internal sealed class LengthSupportedFtpStream : Stream, IWrapper<Stream>
    {
        private long _length;

        /// <inheritdoc/>
        public override bool CanRead => Inner.CanRead;

        /// <inheritdoc/>
        public override bool CanWrite => Inner.CanWrite;

        /// <inheritdoc/>
        public override bool CanSeek => Inner.CanSeek;

        /// <inheritdoc/>
        public override long Length => _length;

        /// <inheritdoc/>
        public override long Position
        {
            get => Inner.Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc/>
        public Stream Inner { get; }

        public LengthSupportedFtpStream(Stream stream, long length)
        {
            Inner = stream;
            _length = length;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException("Stream is not readable.");

            return Inner.Read(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException("Stream is not writable.");

            Inner.Write(buffer, offset, count);
            _length = Math.Max(_length, Position);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            Inner.Flush();
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return Inner.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            Inner.SetLength(value);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Inner.Dispose();

            base.Dispose(disposing);
        }
    }
}
