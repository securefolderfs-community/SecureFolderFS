using Android.OS;
using Java.Nio;
using Java.Nio.Channels;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.Streams
{
    public sealed class ChannelledStream : Stream
    {
        private readonly FileChannel _inputChannel;
        private readonly FileChannel? _outputChannel;
        private readonly ParcelFileDescriptor? _pfd;

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

        public ChannelledStream(FileChannel inputChannel, FileChannel? outputChannel, ParcelFileDescriptor? pfd = null)
        {
            _inputChannel = inputChannel ?? throw new ArgumentNullException(nameof(inputChannel));
            _outputChannel = outputChannel;
            _pfd = pfd;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (!CanRead)
                throw new NotSupportedException("Stream is not readable.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            // Do NOT use ByteBuffer.Wrap(buffer) here. Managed arrays are marshaled to Java
            // by copy, and the copy-back into the managed array happens when the invoked
            // method (Wrap) returns, even before the channel has read anything. The channel would
            // fill the retained Java-side copy while the managed buffer stays untouched.
            using var byteBuffer = ByteBuffer.Allocate(count);
            var read = _inputChannel.Read(byteBuffer);
            if (read <= 0)
                return 0;

            // Copy the data back into the managed buffer
            byteBuffer.Flip();
            byteBuffer.Get(buffer, offset, read);

            return read;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            if (!CanWrite)
                throw new NotSupportedException("Stream is not writable.");

            if (offset < 0 || count < 0 || offset + count > buffer.Length)
                throw new ArgumentOutOfRangeException();

            // Wrap is safe for writing - the managed data is copied into the Java-side
            // array when Wrap is invoked, which is all the write direction needs
            using var byteBuffer = ByteBuffer.Wrap(buffer, offset, count);

            // A single write is not guaranteed to consume the whole buffer
            while (byteBuffer.HasRemaining)
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
                _inputChannel.Close();
                _outputChannel?.Close();
                _pfd?.Close();
            }

            base.Dispose(disposing);
        }
    }
}