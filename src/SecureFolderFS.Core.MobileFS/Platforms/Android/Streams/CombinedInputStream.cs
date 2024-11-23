using Java.IO;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.Streams
{
    public class CombinedInputStream : Stream
    {
        private readonly InputStream _inputStream;
        private readonly OutputStream _outputStream;
        private long _position;
        private readonly long _length;
        private bool _disposed;

        public CombinedInputStream(InputStream inputStream, OutputStream outputStream, long length)
        {
            _inputStream = inputStream;
            _outputStream = outputStream;
            _length = length;
            _position = 0;
        }

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || (value > 0 && _length != -1 && value > _length))
                    throw new ArgumentOutOfRangeException(nameof(value));

                if (value != _position)
                {
                    // Reset input stream and skip to the desired position
                    _inputStream.Reset(); // Resets to the beginning of the stream

                    // Skip to the desired position
                    long skipAmount = value;
                    while (skipAmount > 0)
                    {
                        long skipped = _inputStream.Skip(skipAmount);
                        if (skipped <= 0) break;
                        skipAmount -= skipped;
                    }

                    _position = value;
                }
            }
        }

        public override void Flush()
        {
            _outputStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            // Adjust count if it would read past the end of the stream
            if (_length != -1)
            {
                count = (int)Math.Min(count, _length - _position);
            }

            if (count <= 0) return 0;

            int bytesRead;
            if (offset != 0)
            {
                // Create a temporary buffer if offset is not 0
                var tempBuffer = new byte[count];
                bytesRead = _inputStream.Read(tempBuffer);
                if (bytesRead > 0)
                {
                    Array.Copy(tempBuffer, 0, buffer, offset, bytesRead);
                }
            }
            else
            {
                bytesRead = _inputStream.Read(buffer);
            }

            if (bytesRead > 0)
            {
                _position += bytesRead;
            }

            return bytesRead;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _length + offset,
                _ => throw new ArgumentException("Invalid seek origin")
            };

            Position = newPosition;
            return _position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException("Cannot modify file length on this stream");
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            if (offset != 0)
            {
                // Create a temporary buffer if offset is not 0
                var tempBuffer = new byte[count];
                Array.Copy(buffer, offset, tempBuffer, 0, count);
                _outputStream.Write(tempBuffer, 0, count);
            }
            else
            {
                _outputStream.Write(buffer, 0, count);
            }

            // Update position after writing
            _position += count;

            // If length was unknown, update it
            if (_length == -1)
            {
                _position = Math.Max(_position, _length);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _inputStream.Close();
                    _outputStream.Close();
                }

                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}