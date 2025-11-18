using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;

namespace SecureFolderFS.Sdk.GoogleDrive.Streams
{
    internal sealed class GoogleDriveReadStream : Stream
    {
        private readonly DriveService _service;
        private readonly string _fileId;
        private readonly long _fileSize;
        private long _position;
        private bool _disposed;

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
        {
            _service = service;
            _fileId = fileId;
            _fileSize = fileSize;
            _position = 0;
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GoogleDriveReadStream));

            if (_position >= _fileSize)
                return 0;

            var bytesToRead = (int)Math.Min(count, _fileSize - _position);
            var request = _service.Files.Get(_fileId);

            // Set range header for partial download
            var endByte = _position + bytesToRead - 1;
            using var memoryStream = new MemoryStream();

            var rangeHeader = new System.Net.Http.Headers.RangeHeaderValue(_position, endByte);
            _ = request.DownloadRange(memoryStream, rangeHeader);

            memoryStream.Position = 0;
            var read = memoryStream.Read(buffer.AsSpan(offset, count));

            _position += read;
            return read;
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GoogleDriveReadStream));

            if (_position >= _fileSize)
                return 0;

            var bytesToRead = (int)Math.Min(count, _fileSize - _position);
            var request = _service.Files.Get(_fileId);

            // Set range header for partial download
            var endByte = _position + bytesToRead - 1;
            using var memoryStream = new MemoryStream();

            var rangeHeader = new System.Net.Http.Headers.RangeHeaderValue(_position, endByte);
            _ = await request.DownloadRangeAsync(memoryStream, rangeHeader, cancellationToken);

            memoryStream.Position = 0;
            var read = await memoryStream.ReadAsync(buffer.AsMemory(offset, count), CancellationToken.None);

            _position += read;
            return read;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(GoogleDriveReadStream));

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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
                _disposed = true;

            base.Dispose(disposing);
        }
    }
}