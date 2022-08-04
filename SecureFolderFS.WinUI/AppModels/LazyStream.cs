using System.IO;

namespace SecureFolderFS.WinUI.AppModels
{
    /// <summary>
    /// A stream that is lazily initialized by passing <see cref="LateStream"/>.
    /// </summary>
    internal sealed class LazyStream : Stream
    {
        public Stream? LateStream { get; set; }

        /// <inheritdoc/>
        public override bool CanRead => LateStream?.CanRead ?? false;

        /// <inheritdoc/>
        public override bool CanSeek => LateStream?.CanSeek ?? false;

        /// <inheritdoc/>
        public override bool CanWrite => LateStream?.CanWrite ?? false;

        /// <inheritdoc/>
        public override long Length => LateStream?.Length ?? 0L;

        /// <inheritdoc/>
        public override long Position
        {
            get => LateStream?.Position ?? 0;
            set
            {
                if (LateStream is not null)
                    LateStream.Position = value;
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            LateStream?.Flush();
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return LateStream?.Read(buffer, offset, count) ?? 0;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            return LateStream?.Seek(offset, origin) ?? 0L;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            LateStream?.SetLength(value);
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            LateStream?.Write(buffer, offset, count);
        }
    }
}
