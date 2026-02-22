using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;

namespace SecureFolderFS.Sdk.GoogleDrive.Streams
{
    internal sealed class GoogleDriveReadStream : Stream
    {
        /// <summary>
        /// The default buffer size for read-ahead caching (1024 KB).
        /// This reduces the number of HTTP requests by pre-fetching larger chunks.
        /// </summary>
        private const int DEFAULT_BUFFER_SIZE = 4 * (256 * 1024);

        /// <summary>
        /// Minimum buffer size to avoid excessive allocations for tiny reads.
        /// </summary>
        private const int MIN_BUFFER_SIZE = 4 * 1024; // 4 KB

        private readonly long _fileSize;
        private readonly FilesResource.GetRequest _cachedRequest;
        private readonly int _bufferSize;
        private long _position;
        private bool _disposed;

        // Read-ahead buffer for caching
        private byte[]? _readBuffer;
        private long _bufferStartPosition;
        private int _bufferValidLength;

        /// <inheritdoc/>
        public override bool CanRead { get; } = true;

        /// <inheritdoc/>
        public override bool CanSeek { get; } = true;

        /// <inheritdoc/>
        public override bool CanWrite { get; } = false;

        /// <inheritdoc/>
        public override long Length => _fileSize;

        /// <inheritdoc/>
        public override long Position
        {
            get => _position;
            set
            {
                if (value < 0 || value > _fileSize)
                    throw new ArgumentOutOfRangeException(nameof(value));

                _position = value;
            }
        }

        public GoogleDriveReadStream(DriveService service, string fileId, long fileSize)
            : this(service.Files.Get(fileId), fileSize)
        {
        }

        public GoogleDriveReadStream(FilesResource.GetRequest request, long fileSize)
        {
            _cachedRequest = request;
            _fileSize = fileSize;
            _bufferSize = CalculateOptimalBufferSize(fileSize);
            _position = 0;
            _bufferStartPosition = -1;
            _bufferValidLength = 0;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_position >= _fileSize)
                return 0;

            var totalRead = 0;

            while (count > 0 && _position < _fileSize)
            {
                // Check if we can serve from buffer
                if (IsPositionInBuffer(_position))
                {
                    var bufferOffset = (int)(_position - _bufferStartPosition);
                    var availableInBuffer = _bufferValidLength - bufferOffset;
                    var toRead = Math.Min(count, availableInBuffer);

                    Array.Copy(_readBuffer!, bufferOffset, buffer, offset, toRead);

                    _position += toRead;
                    offset += toRead;
                    count -= toRead;
                    totalRead += toRead;
                }
                else
                {
                    // Need to fetch new data - fetch a larger chunk for read-ahead
                    FillBuffer(_position);

                    // If buffer is still empty after fill, we've reached end of file
                    if (_bufferValidLength == 0)
                        break;
                }
            }

            return totalRead;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_position >= _fileSize)
                return 0;

            var totalRead = 0;

            while (count > 0 && _position < _fileSize)
            {
                // Check if we can serve from buffer
                if (IsPositionInBuffer(_position))
                {
                    var bufferOffset = (int)(_position - _bufferStartPosition);
                    var availableInBuffer = _bufferValidLength - bufferOffset;
                    var toRead = Math.Min(count, availableInBuffer);

                    Array.Copy(_readBuffer!, bufferOffset, buffer, offset, toRead);

                    _position += toRead;
                    offset += toRead;
                    count -= toRead;
                    totalRead += toRead;
                }
                else
                {
                    // Need to fetch new data - fetch a larger chunk for read-ahead
                    await FillBufferAsync(_position, cancellationToken);

                    // If buffer is still empty after fill, we've reached end of file
                    if (_bufferValidLength == 0)
                        break;
                }
            }

            return totalRead;
        }

        private bool IsPositionInBuffer(long position)
        {
            return _readBuffer is not null
                   && position >= _bufferStartPosition
                   && position < _bufferStartPosition + _bufferValidLength;
        }

        private void FillBuffer(long startPosition)
        {
            _readBuffer ??= new byte[_bufferSize];
            _bufferStartPosition = startPosition;

            var bytesToRead = (int)Math.Min(_bufferSize, _fileSize - startPosition);
            if (bytesToRead <= 0)
            {
                _bufferValidLength = 0;
                return;
            }

            var endByte = startPosition + bytesToRead - 1;
            using var memoryStream = new MemoryStream();

            var rangeHeader = new System.Net.Http.Headers.RangeHeaderValue(startPosition, endByte);
            _ = _cachedRequest.DownloadRange(memoryStream, rangeHeader);

            memoryStream.Position = 0;
            _bufferValidLength = memoryStream.Read(_readBuffer, 0, bytesToRead);
        }

        private async Task FillBufferAsync(long startPosition, CancellationToken cancellationToken)
        {
            _readBuffer ??= new byte[_bufferSize];
            _bufferStartPosition = startPosition;

            var bytesToRead = (int)Math.Min(_bufferSize, _fileSize - startPosition);
            if (bytesToRead <= 0)
            {
                _bufferValidLength = 0;
                return;
            }

            var endByte = startPosition + bytesToRead - 1;
            using var memoryStream = new MemoryStream();

            var rangeHeader = new System.Net.Http.Headers.RangeHeaderValue(startPosition, endByte);
            _ = await _cachedRequest.DownloadRangeAsync(memoryStream, rangeHeader, cancellationToken);

            memoryStream.Position = 0;
            _bufferValidLength = await memoryStream.ReadAsync(_readBuffer.AsMemory(0, bytesToRead), CancellationToken.None);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => _fileSize + offset,
                _ => throw new ArgumentException("Invalid seek origin.", nameof(origin))
            };

            if (newPosition < 0 || newPosition > _fileSize)
                throw new IOException("Seek position is out of range.");

            _position = newPosition;
            return _position;
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Calculates the optimal buffer size based on file size.
        /// For small files, uses the file size to fetch everything in one request.
        /// For larger files, uses the default buffer size.
        /// </summary>
        private static int CalculateOptimalBufferSize(long fileSize)
        {
            // For files smaller than or equal to the default buffer, use file size
            // This allows fetching the entire file in a single request
            if (fileSize <= DEFAULT_BUFFER_SIZE)
                return (int)Math.Max(fileSize, MIN_BUFFER_SIZE);

            return DEFAULT_BUFFER_SIZE;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposed = true;
                _readBuffer = null;
            }

            base.Dispose(disposing);
        }
    }
}