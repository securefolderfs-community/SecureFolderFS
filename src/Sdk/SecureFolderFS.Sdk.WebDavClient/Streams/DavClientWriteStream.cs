using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Streams
{
    internal sealed class DavClientWriteStream : Stream
    {
        private readonly IWebDavClient _client;
        private readonly Uri _baseUri;
        private readonly string _path;
        private readonly MemoryStream _buffer = new();
        private bool _disposed;

        public DavClientWriteStream(IWebDavClient client, Uri baseUri, string path)
        {
            _client = client;
            _baseUri = baseUri;
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
            var uri = new Uri(_baseUri, _path);
            var response = await _client.PutFile(uri, _buffer);

            if (!response.IsSuccessful)
                throw new IOException($"Failed to upload file '{_path}': {response.StatusCode}");
        }
    }
}

