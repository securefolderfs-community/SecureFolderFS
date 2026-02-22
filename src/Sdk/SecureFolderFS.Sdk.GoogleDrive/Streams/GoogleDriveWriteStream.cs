using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using Google.Apis.Upload;
using File = Google.Apis.Drive.v3.Data.File;

namespace SecureFolderFS.Sdk.GoogleDrive.Streams
{
    internal sealed class GoogleDriveWriteStream : Stream
    {
        /// <summary>
        /// The default chunk size for buffered writes (1024 KB).
        /// Google Drive API recommends chunk sizes to be multiples of 256 KB.
        /// </summary>
        private const int DEFAULT_CHUNK_SIZE = 4 * (256 * 1024);

        /// <summary>
        /// Threshold for using resumable upload vs simple upload.
        /// Files larger than this will use resumable upload for better reliability.
        /// </summary>
        private const int RESUMABLE_UPLOAD_THRESHOLD = 5 * 1024 * 1024; // 5 MB

        private readonly DriveService _service;
        private readonly string _fileId;
        private readonly string _name;
        private readonly string _mimeType;
        private readonly int _chunkSize;
        private readonly MemoryStream _writeBuffer;
        private long _totalBytesWritten;
        private bool _disposed;
        private bool _isDirty;

        /// <inheritdoc/>
        public override bool CanRead { get; } = false;

        /// <inheritdoc/>
        public override bool CanSeek { get; } = true;

        /// <inheritdoc/>
        public override bool CanWrite { get; } = true;

        /// <inheritdoc/>
        public override long Length => _totalBytesWritten + _writeBuffer.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _totalBytesWritten + _writeBuffer.Position;
            set
            {
                if (value < _totalBytesWritten)
                    throw new NotSupportedException("Cannot seek backwards past already uploaded data.");

                _writeBuffer.Position = value - _totalBytesWritten;
            }
        }

        public GoogleDriveWriteStream(DriveService service, string fileId, string name, string mimeType, int chunkSize = DEFAULT_CHUNK_SIZE)
        {
            _service = service;
            _fileId = fileId;
            _name = name;
            _mimeType = mimeType;
            _chunkSize = chunkSize;
            _writeBuffer = new MemoryStream();
            _totalBytesWritten = 0;
            _isDirty = false;
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _writeBuffer.Write(buffer, offset, count);
            _isDirty = true;
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _isDirty = true;
            return _writeBuffer.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _isDirty = true;
            return _writeBuffer.WriteAsync(buffer, cancellationToken);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            FlushAsync(CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!_isDirty || _writeBuffer.Length == 0)
                return;

            await UploadBufferAsync(cancellationToken);
            _isDirty = false;
        }

        private async Task UploadBufferAsync(CancellationToken cancellationToken)
        {
            _writeBuffer.Position = 0;

            var fileMeta = new File() { Name = _name };
            var updateRequest = _service.Files.Update(fileMeta, _fileId, _writeBuffer, _mimeType);
            updateRequest.Fields = "id";

            // Use larger chunk size for resumable upload to reduce HTTP round trips
            if (_writeBuffer.Length > RESUMABLE_UPLOAD_THRESHOLD)
            {
                // Set chunk size for resumable upload (must be multiple of 256KB)
                updateRequest.ChunkSize = Math.Max(_chunkSize, ResumableUpload.MinimumChunkSize);
            }

            var progress = await updateRequest.UploadAsync(cancellationToken);
            if (progress.Status == UploadStatus.Failed)
                throw new IOException($"Upload failed: {progress.Exception?.Message}.", progress.Exception);

            _totalBytesWritten += _writeBuffer.Length;

            // Reset the buffer for next use
            _writeBuffer.SetLength(0);
            _writeBuffer.Position = 0;
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _writeBuffer.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (value < _totalBytesWritten)
                throw new NotSupportedException("Cannot set length smaller than already uploaded data.");

            _writeBuffer.SetLength(value - _totalBytesWritten);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                try
                {
                    Flush();
                }
                finally
                {
                    _writeBuffer.Dispose();
                    _disposed = true;
                }
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                try
                {
                    await FlushAsync(CancellationToken.None);
                }
                finally
                {
                    await _writeBuffer.DisposeAsync();
                    _disposed = true;
                }
            }

            await base.DisposeAsync();
        }
    }
}
