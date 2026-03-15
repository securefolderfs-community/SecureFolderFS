using Dropbox.Api;
using Dropbox.Api.Files;

namespace SecureFolderFS.Sdk.Dropbox.Streams
{
    internal sealed class DropboxWriteStream : Stream
    {
        // Dropbox recommends session upload for files > 150 MB,
        // but even a modest threshold avoids large single-shot uploads.
        private const int SESSION_THRESHOLD_BYTES = 150 * 1024 * 1024;

        private readonly DropboxClient _client;
        private readonly string _path;
        private readonly MemoryStream _buffer = new();
        private bool _disposed;

        public DropboxWriteStream(DropboxClient client, string path)
        {
            _client = client;
            _path = path;
        }

        /// <inheritdoc/>
        public override bool CanRead => false;

        /// <inheritdoc/>
        public override bool CanSeek => false;

        /// <inheritdoc/>
        public override bool CanWrite => true;

        /// <inheritdoc/>
        public override long Length => _buffer.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _buffer.Position;
            set => throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void SetLength(long value) => throw new NotSupportedException();

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) => _buffer.Write(buffer, offset, count);

        /// <inheritdoc/>
        public override void Write(ReadOnlySpan<byte> buffer) => _buffer.Write(buffer);

        /// <inheritdoc/>
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken ct) =>
            _buffer.WriteAsync(buffer, offset, count, ct);

        /// <inheritdoc/>
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken ct = default) =>
            _buffer.WriteAsync(buffer, ct);

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                UploadAsync().GetAwaiter().GetResult();
                _buffer.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                await UploadAsync();
                await _buffer.DisposeAsync();
            }

            await base.DisposeAsync();
        }

        private async Task UploadAsync()
        {
            _buffer.Position = 0;
            var size = _buffer.Length;
            if (size <= SESSION_THRESHOLD_BYTES)
            {
                // Simple upload - single request
                await _client.Files.UploadAsync(
                    _path,
                    WriteMode.Overwrite.Instance,
                    body: _buffer);
            }
            else
            {
                // Session upload - stream in 150 MB chunks
                const int CHUNK_SIZE = SESSION_THRESHOLD_BYTES;
                var buffer = new byte[CHUNK_SIZE];

                // Start a session with the first chunk
                var read = await _buffer.ReadAsync(buffer.AsMemory(0, CHUNK_SIZE));
                var session = await _client.Files.UploadSessionStartAsync(body: new MemoryStream(buffer, 0, read));
                var sessionId = session.SessionId;
                long offset = read;

                // Continue uploading chunks
                while (offset < size)
                {
                    read = await _buffer.ReadAsync(buffer, 0, CHUNK_SIZE);
                    var isLast = _buffer.Position >= size;

                    if (isLast)
                    {
                        await _client.Files.UploadSessionFinishAsync(
                            new UploadSessionCursor(sessionId, (ulong)offset),
                            new CommitInfo(_path, WriteMode.Overwrite.Instance),
                            body: new MemoryStream(buffer, 0, read));
                    }
                    else
                    {
                        await _client.Files.UploadSessionAppendV2Async(
                            new UploadSessionCursor(sessionId, (ulong)offset),
                            body: new MemoryStream(buffer, 0, read));
                        offset += read;
                    }
                }
            }
        }
    }
}