using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.WebDavClient.Streams
{
    /// <summary>
    /// A read-only seekable stream over a WebDAV resource that uses HTTP Range requests
    /// to support arbitrary seeking without buffering the full content in memory.
    ///
    /// On creation, a HEAD request is issued to determine <see cref="Length"/>.
    /// Forward reads reuse the current connection. Any call to <see cref="Seek"/>
    /// tears down the current response stream and opens a new ranged GET from the
    /// requested position.
    /// </summary>
    internal sealed class DavClientReadStream : Stream
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _fileUri;
        private Stream? _inner;
        private long _position;
        private long? _pendingSeekPosition;
        private bool _disposed;

        /// <inheritdoc/>
        public override bool CanRead => !_disposed;

        /// <inheritdoc/>
        public override bool CanSeek => !_disposed;

        /// <inheritdoc/>
        public override bool CanWrite => false;

        /// <inheritdoc/>
        public override long Length { get; }

        /// <inheritdoc/>
        public override long Position
        {
            get => _position;
            set => Seek(value, SeekOrigin.Begin);
        }

        private DavClientReadStream(HttpClient httpClient, Uri fileUri, Stream initialStream, long length)
        {
            _httpClient = httpClient;
            _fileUri = fileUri;
            _inner = initialStream;
            Length = length;
            _position = 0;
        }

        /// <summary>
        /// Creates a <see cref="DavClientReadStream"/> by issuing a HEAD request for
        /// <see cref="Length"/> and an initial GET request for the full stream body.
        /// </summary>
        public static async Task<DavClientReadStream> CreateAsync(
            HttpClient httpClient,
            Uri fileUri,
            CancellationToken cancellationToken)
        {
            // HEAD request to determine Content-Length
            var length = -1L;
            try
            {
                using var headRequest = new HttpRequestMessage(HttpMethod.Head, fileUri);
                using var headResponse = await httpClient.SendAsync(headRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
                if (headResponse.IsSuccessStatusCode)
                {
                    // Content.Headers.ContentLength is null when Apache omits the header
                    // (e.g. chunked transfer). Fall back to the raw header string.
                    length = headResponse.Content.Headers.ContentLength
                             ?? (headResponse.Headers.TryGetValues("Content-Length", out var values)
                                 && long.TryParse(System.Linq.Enumerable.FirstOrDefault(values), out var parsed)
                                 ? parsed : -1L);
                }
            }
            catch
            {
                // Length remains -1 (unknown) - seeking will still work positionally,
                // but SeekOrigin.End will not be reliable
            }

            // Initial GET for full content from position 0
            var initialStream = await OpenRangeStreamAsync(httpClient, fileUri, 0, cancellationToken);
            return new DavClientReadStream(httpClient, fileUri, initialStream, length);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            var newPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => _position + offset,
                SeekOrigin.End => Length + offset, // unreliable if Length == -1
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };

            if (newPosition < 0)
                throw new IOException("Cannot seek before the beginning of the stream.");

            if (newPosition == _position)
                return _position;

            // Schedule disposal of the current inner stream asynchronously.
            // Seek is synchronous, so we can't await - fire and forget is acceptable
            // here since we're just releasing the HTTP response body connection.
            var oldStream = _inner;
            _inner = null;
            oldStream?.DisposeAsync().AsTask().ContinueWith(_ => { }, TaskContinuationOptions.None);

            _position = newPosition;
            _pendingSeekPosition = newPosition;

            return _position;
        }

        // Ensures the inner stream is open and positioned correctly before any read.
        // Called at the start of every Read* method.
        [MemberNotNull(nameof(_inner))]
        private async Task EnsureStreamAtPositionAsync(CancellationToken cancellationToken)
        {
            if (_pendingSeekPosition is not null)
            {
                _inner = await OpenRangeStreamAsync(_httpClient, _fileUri, _pendingSeekPosition.Value, cancellationToken);
                _pendingSeekPosition = null;
            }
            else if (_inner is null)
            {
                _inner = await OpenRangeStreamAsync(_httpClient, _fileUri, _position, cancellationToken);
            }
        }

        private static async Task<Stream> OpenRangeStreamAsync(
            HttpClient httpClient,
            Uri fileUri,
            long fromPosition,
            CancellationToken cancellationToken)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, fileUri);

            // Only set Range header if not reading from the start - some servers
            // behave differently for Range: bytes=0- vs. a plain GET
            if (fromPosition > 0L)
                request.Headers.Range = new RangeHeaderValue(fromPosition, null);

            var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadAsStreamAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return ReadAsync(buffer, offset, count, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <inheritdoc/>
        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            await EnsureStreamAtPositionAsync(cancellationToken);

            var totalRead = 0;
            while (totalRead < count)
            {
                var bytesRead = await _inner.ReadAsync(buffer.AsMemory(offset + totalRead, count - totalRead), cancellationToken);
                if (bytesRead == 0)
                    break; // EOF

                totalRead += bytesRead;
            }

            _position += totalRead;
            return totalRead;
        }

        /// <inheritdoc/>
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            await EnsureStreamAtPositionAsync(cancellationToken);

            var totalRead = 0;
            while (totalRead < buffer.Length)
            {
                var bytesRead = await _inner.ReadAsync(buffer.Slice(totalRead), cancellationToken);
                if (bytesRead == 0)
                    break; // EOF

                totalRead += bytesRead;
            }

            _position += totalRead;
            return totalRead;
        }

        /// <inheritdoc/>
        public override int ReadByte()
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            EnsureStreamAtPositionAsync(CancellationToken.None).GetAwaiter().GetResult();

            var result = _inner.ReadByte();
            if (result != -1)
                _position++;

            return result;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            throw new NotSupportedException("This stream does not support SetLength.");
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException("This stream does not support writing.");
        }

        /// <inheritdoc/>
        public override void Flush()
        {
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
                _inner?.Dispose();

            _disposed = true;
            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            if (_inner is not null)
                await _inner.DisposeAsync();

            _disposed = true;
            await base.DisposeAsync();
        }
    }
}