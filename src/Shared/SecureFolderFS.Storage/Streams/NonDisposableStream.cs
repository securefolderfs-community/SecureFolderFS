using System.IO;
using System.Threading.Tasks;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Storage.Streams
{
    public class NonDisposableStream : Stream, IWrapper<Stream>
    {
        private readonly bool? _canSeek;
        private readonly bool? _canRead;
        private readonly bool? _canWrite;

        /// <inheritdoc/>
        public Stream Inner { get; }

        /// <inheritdoc/>
        public override bool CanSeek => _canSeek ?? Inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanRead => _canRead ?? Inner.CanRead;

        /// <inheritdoc/>
        public override bool CanWrite => _canWrite ?? Inner.CanWrite;

        /// <inheritdoc/>
        public override long Length => Inner.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => Inner.Position;
            set => Inner.Position = value;
        }

        public NonDisposableStream(Stream inner, bool? canSeek = null, bool? canRead = null, bool? canWrite = null)
        {
            Inner = inner;
            _canSeek = canSeek;
            _canRead = canRead;
            _canWrite = canWrite;
        }

        /// <inheritdoc/>
        public override void Flush() => Inner.Flush();

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => Inner.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => Inner.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value) => Inner.SetLength(value);

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => Inner.Write(buffer, offset, count);

        /// <summary>
        /// Forces the closure of the stream and releases the associated resources.
        /// </summary>
        public void ForceClose()
        {
            base.Dispose(true);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            _ = disposing;
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync()
        {
            return default;
        }
    }
}
