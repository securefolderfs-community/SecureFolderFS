using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Streams
{
    /// <summary>
    /// A read/write/seekable stream over a WebDAV resource that buffers the full
    /// file content in memory. On creation the existing file is downloaded so that
    /// partial/offset writes are merged correctly. On disposal the full buffer is
    /// PUT back to the server.
    /// </summary>
    internal sealed class DavClientWriteStream : Stream
    {
        private readonly IWebDavClient _client;
        private readonly Uri _fileUri;
        private readonly MemoryStream _buffer;
        private bool _disposed;

        /// <inheritdoc/>
        public override bool CanRead => !_disposed;

        /// <inheritdoc/>
        public override bool CanSeek => !_disposed;

        /// <inheritdoc/>
        public override bool CanWrite => !_disposed;

        /// <inheritdoc/>
        public override long Length => _buffer.Length;

        /// <inheritdoc/>
        public override long Position
        {
            get => _buffer.Position;
            set => _buffer.Position = value;
        }

        private DavClientWriteStream(IWebDavClient client, Uri fileUri, MemoryStream buffer)
        {
            _client = client;
            _fileUri = fileUri;
            _buffer = buffer;
        }

        /// <summary>
        /// Creates a <see cref="DavClientWriteStream"/> by downloading the existing
        /// file content (if any) into an in-memory buffer so that partial/seeked
        /// writes are merged correctly before upload.
        /// </summary>
        public static async Task<DavClientWriteStream> CreateAsync(
            HttpClient httpClient,
            IWebDavClient client,
            Uri fileUri,
            FileAccess accessMode,
            CancellationToken cancellationToken = default)
        {
            var buffer = new MemoryStream();

            if (accessMode == FileAccess.ReadWrite)
            {
                try
                {
                    var request = new HttpRequestMessage(HttpMethod.Get, fileUri);
                    var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

                    if (response.IsSuccessStatusCode)
                        await (await response.Content.ReadAsStreamAsync(cancellationToken)).CopyToAsync(buffer, cancellationToken);
                }
                catch
                {
                    // File likely doesn't exist yet - start with an empty buffer
                }
            }

            buffer.Position = 0L;
            return new DavClientWriteStream(client, fileUri, buffer);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin) => _buffer.Seek(offset, origin);

        /// <inheritdoc/>
        public override void SetLength(long value) => _buffer.SetLength(value);

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) => _buffer.Read(buffer, offset, count);

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer) => _buffer.Read(buffer);

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
        public override void Flush() => _buffer.Flush();

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _disposed = true;
                Task.Run(UploadAsync).GetAwaiter().GetResult();
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
            // ToArray() returns exactly [0..Length], regardless of Position or capacity.
            // This guarantees truncations are reflected - passing MemoryStream directly
            // risks the WebDAV library reading stale bytes beyond the truncated Length
            // if it uses the underlying buffer rather than the stream bounds.
            var data = _buffer.ToArray();
            using var uploadStream = new MemoryStream(data, writable: false);

            var response = await _client.PutFile(_fileUri, uploadStream);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to upload file '{_fileUri}': {response.StatusCode}");
        }
    }
}