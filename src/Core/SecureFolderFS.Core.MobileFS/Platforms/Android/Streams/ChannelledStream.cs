using Java.Nio;
using Java.Nio.Channels;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.Streams
{
    public sealed class ChannelledStream : Stream
    {
        private readonly FileChannel _inputChannel;
        private readonly FileChannel? _outputChannel;

        /// <inheritdoc/>
        public override bool CanRead => _inputChannel.IsOpen;

        /// <inheritdoc/>
        public override bool CanSeek => _inputChannel.IsOpen && (_outputChannel?.IsOpen ?? true);

        /// <inheritdoc/>
        public override bool CanWrite => _outputChannel?.IsOpen ?? false;

        /// <inheritdoc/>
        public override long Length => _inputChannel.Size();

        /// <inheritdoc/>
        public override long Position
        {
            get => _inputChannel.Position();
            set
            {
                _inputChannel.Position(value);
                _outputChannel?.Position(value);
            }
        }

        public ChannelledStream(FileChannel inputChannel, FileChannel? outputChannel)
        {
            _inputChannel = inputChannel ?? throw new ArgumentNullException(nameof(inputChannel));
            _outputChannel = outputChannel;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException("Stream is not readable.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            var byteBuffer = ByteBuffer.Wrap(buffer, offset, count);
            var read = _inputChannel.Read(byteBuffer);
            if (read < 0)
                return 0;

            return read;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException("Stream is not writable.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            var byteBuffer = ByteBuffer.Wrap(buffer, offset, count);
            _outputChannel!.Write(byteBuffer);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin), origin, null)
            };

            _inputChannel.Position(newPosition);
            _outputChannel?.Position(newPosition);
            return newPosition;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            if (!CanWrite)
                throw new NotSupportedException("Stream is not writable.");

            _outputChannel!.Truncate(value);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            if (!CanWrite)
                throw new NotSupportedException("Stream is not writable.");

            _outputChannel!.Force(false);
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _inputChannel?.Close();
                _outputChannel?.Close();
            }

            base.Dispose(disposing);
        }
    }
}