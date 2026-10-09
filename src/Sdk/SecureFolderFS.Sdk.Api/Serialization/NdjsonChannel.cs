using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Api.Protocol;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.Api.Serialization
{
    /// <summary>
    /// Reads and writes <see cref="ApiMessage"/> instances as newline-delimited JSON over a stream.
    /// </summary>
    /// <remarks>
    /// One compact JSON object per line, terminated by <c>\n</c>. Each message is serialized through the
    /// shared <see cref="IAsyncSerializer{TSerialized}"/>; this type only owns the framing. Reads are
    /// bounded by <see cref="Constants.Limits.MAX_MESSAGE_BYTES"/> so an attacker cannot force unbounded buffering.
    /// </remarks>
    public sealed class NdjsonChannel : IDisposable
    {
        private const byte NEWLINE = (byte)'\n';
        private const byte CARRIAGE_RETURN = (byte)'\r';
        private const int INITIAL_BUFFER_SIZE = 4096;

        private static readonly byte[] NewlineBuffer = [NEWLINE];

        private readonly Stream _stream;
        private readonly IAsyncSerializer<Stream> _serializer;
        private readonly int _maxMessageBytes;
        private readonly SemaphoreSlim _writeLock;
        private byte[] _buffer;
        private int _bufferedLength;
        private bool _disposed;

        public NdjsonChannel(
            Stream stream,
            IAsyncSerializer<Stream>? serializer = null,
            int maxMessageBytes = Constants.Limits.MAX_MESSAGE_BYTES)
        {
            _stream = stream;
            _serializer = serializer ?? ApiSerializer.Instance;
            _maxMessageBytes = maxMessageBytes;
            _writeLock = new SemaphoreSlim(1, 1);
            _buffer = new byte[INITIAL_BUFFER_SIZE];
        }

        /// <summary>
        /// Reads the next message, or <see langword="null"/> when the peer closed the connection.
        /// </summary>
        public async Task<ApiMessage?> ReadAsync(CancellationToken cancellationToken = default)
        {
            while (true)
            {
                var newlineIndex = Array.IndexOf(_buffer, NEWLINE, 0, _bufferedLength);
                if (newlineIndex >= 0)
                {
                    var consumed = newlineIndex + 1;
                    var lineLength = newlineIndex;

                    // Tolerate CRLF from clients written against line-oriented APIs
                    if (lineLength > 0 && _buffer[lineLength - 1] == CARRIAGE_RETURN)
                        lineLength--;

                    // Blank lines are permitted as keep-alives
                    if (lineLength == 0)
                    {
                        Consume(consumed);
                        continue;
                    }

                    try
                    {
                        return await DeserializeAsync(lineLength, cancellationToken);
                    }
                    finally
                    {
                        // Discard the line even when parsing throws, otherwise one bad line loops forever.
                        Consume(consumed);
                    }
                }

                // No delimiter yet, and the peer has already spent the whole budget on one line
                if (_bufferedLength >= _maxMessageBytes)
                {
                    throw new ApiProtocolException(
                        Constants.ErrorCodes.INVALID_REQUEST,
                        $"Message exceeded the {_maxMessageBytes} byte limit.",
                        isFatal: true);
                }

                if (_bufferedLength == _buffer.Length)
                    Array.Resize(ref _buffer, Math.Min(_buffer.Length * 2, _maxMessageBytes + 1));

                var read = await _stream.ReadAsync(_buffer.AsMemory(_bufferedLength), cancellationToken);
                if (read == 0)
                    return null;

                _bufferedLength += read;
            }
        }

        /// <summary>
        /// Writes a message to the peer. Serialization happens under the write lock so concurrent
        /// notifications and responses can never interleave bytes.
        /// </summary>
        public async Task WriteAsync(ApiMessage message, CancellationToken cancellationToken = default)
        {
            await _writeLock.WaitAsync(cancellationToken);
            try
            {
                await using var payload = await _serializer.SerializeAsync<Stream, ApiMessage>(message, cancellationToken);

                await payload.CopyToAsync(_stream, cancellationToken);
                await _stream.WriteAsync(NewlineBuffer, cancellationToken);
                await _stream.FlushAsync(cancellationToken);
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task<ApiMessage> DeserializeAsync(int lineLength, CancellationToken cancellationToken)
        {
            try
            {
                using var lineStream = new MemoryStream(_buffer, 0, lineLength, writable: false);

                return await _serializer.DeserializeAsync<Stream, ApiMessage>(lineStream, cancellationToken)
                    ?? throw new ApiProtocolException(
                        Constants.ErrorCodes.INVALID_REQUEST, "Message was not a JSON object.", isFatal: false);
            }
            catch (JsonException ex)
            {
                throw new ApiProtocolException(
                    Constants.ErrorCodes.PARSE_ERROR, $"Message was not valid JSON: {ex.Message}", isFatal: false);
            }
        }

        private void Consume(int count)
        {
            var remaining = _bufferedLength - count;
            if (remaining > 0)
                Buffer.BlockCopy(_buffer, count, _buffer, 0, remaining);

            _bufferedLength = remaining;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _writeLock.Dispose();
        }
    }

    /// <summary>
    /// A violation of the wire protocol by the remote peer.
    /// </summary>
    public sealed class ApiProtocolException : Exception
    {
        /// <summary>
        /// Gets the error code from <see cref="Constants.ErrorCodes"/> to report to the peer.
        /// </summary>
        public string Code { get; }

        /// <summary>
        /// Gets whether the connection must be torn down. A malformed message is recoverable; an
        /// oversized one is not, because the framing is no longer trustworthy.
        /// </summary>
        public bool IsFatal { get; }

        public ApiProtocolException(string code, string message, bool isFatal)
            : base(message)
        {
            Code = code;
            IsFatal = isFatal;
        }
    }
}
