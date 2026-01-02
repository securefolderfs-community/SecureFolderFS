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
        private readonly DriveService _service;
        private readonly string _fileId;
        private readonly string _name;
        private readonly string _mimeType;
        private readonly MemoryStream _memoryStreamBuffer;
        private bool _disposed;

        /// <inheritdoc/>
        public override bool CanRead { get; } = false;

        /// <inheritdoc/>
        public override bool CanSeek { get; } = true;

        /// <inheritdoc/>
        public override bool CanWrite { get; } = true;

        /// <inheritdoc/>
        public override long Length => _memoryStreamBuffer.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _memoryStreamBuffer.Position;
            set => _memoryStreamBuffer.Position = value;
        }

        public GoogleDriveWriteStream(DriveService service, string fileId, string name, string mimeType)
        {
            _service = service;
            _fileId = fileId;
            _name = name;
            _mimeType = mimeType;
            _memoryStreamBuffer = new MemoryStream();
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _memoryStreamBuffer.Write(buffer, offset, count);
        }

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _memoryStreamBuffer.WriteAsync(buffer, offset, count, cancellationToken);
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_memoryStreamBuffer.Length == 0)
                return;

            _memoryStreamBuffer.Position = 0;
            var fileMeta = new File() { Name = _name };
            var updateRequest = _service.Files.Update(fileMeta, _fileId, _memoryStreamBuffer, _mimeType);
            updateRequest.Fields = "id";

            var progress = updateRequest.Upload();
            if (progress.Status == UploadStatus.Failed)
                throw new IOException($"Upload failed: {progress.Exception?.Message}.", progress.Exception);
        }

        /// <inheritdoc/>
        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_memoryStreamBuffer.Length == 0)
                return;

            _memoryStreamBuffer.Position = 0;
            var fileMeta = new File() { Name = _name };
            var updateRequest = _service.Files.Update(fileMeta, _fileId, _memoryStreamBuffer, _mimeType);
            updateRequest.Fields = "id";

            var progress = await updateRequest.UploadAsync(cancellationToken);
            if (progress.Status == UploadStatus.Failed)
                throw new IOException($"Upload failed: {progress.Exception?.Message}.", progress.Exception);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            return _memoryStreamBuffer.Seek(offset, origin);
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            _memoryStreamBuffer.SetLength(value);
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
                Flush();
                _memoryStreamBuffer.Dispose();
                _disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
