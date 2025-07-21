using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

namespace SecureFolderFS.Core.FileSystem.Streams
{
    /// <inheritdoc cref="PlaintextStream"/>
    internal sealed class PlaintextStream : Stream, IWrapper<Stream>
    {
        private readonly Security _security;
        private readonly ChunkAccess _chunkAccess;
        private readonly HeaderBuffer _headerBuffer;
        private readonly Action<Stream> _notifyStreamClosed;
        private readonly Lock _writeLock = new();

        private long _Length;
        private long _Position;

        /// <inheritdoc/>
        public Stream Inner { get; }

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override bool CanRead => Inner.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => Inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => Inner.CanWrite;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public PlaintextStream(
            Stream ciphertextStream,
            Security security,
            ChunkAccess chunkAccess,
            HeaderBuffer headerBuffer,
            Action<Stream> notifyStreamClosed)
        {
            Inner = ciphertextStream;
            _security = security;
            _chunkAccess = chunkAccess;
            _headerBuffer = headerBuffer;
            _notifyStreamClosed = notifyStreamClosed;

            if (CanSeek)
                _Length = _security.ContentCrypt.CalculatePlaintextSize(Math.Max(0L, ciphertextStream.Length - _security.HeaderCrypt.HeaderCiphertextSize));
        }

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count)
        {
            return Read(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count)
        {
            Write(buffer.AsSpan(offset, count));
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            if (!CanSeek)
                return 0;

            var ciphertextStreamLength = Inner.Length;
            if (ciphertextStreamLength == 0L)
                return FileSystem.Constants.FILE_EOF;

            if (ciphertextStreamLength < _security.HeaderCrypt.HeaderCiphertextSize)
                return FileSystem.Constants.FILE_EOF; // TODO: HealthAPI - report invalid header size

            var lengthToEof = Length - _Position;
            if (lengthToEof <= 0L)
                return FileSystem.Constants.FILE_EOF;

            // Read header if not ready
            if (!TryReadHeader())
                throw new CryptographicException();

            var read = 0;
            var positionInBuffer = 0;
            var plaintextChunkSize = _security.ContentCrypt.ChunkPlaintextSize;
            var adjustedBuffer = buffer.Slice(0, (int)Math.Min(buffer.Length, lengthToEof));

            while (positionInBuffer < adjustedBuffer.Length)
            {
                var readPosition = _Position + read;
                var chunkNumber = readPosition / plaintextChunkSize;
                var offsetInChunk = (int)(readPosition % plaintextChunkSize);
                var length = Math.Min(adjustedBuffer.Length - positionInBuffer, plaintextChunkSize - offsetInChunk);

                var copied = _chunkAccess.CopyFromChunk(chunkNumber, adjustedBuffer.Slice(positionInBuffer), offsetInChunk);
                if (copied < 0)
                    throw new CryptographicException();

                positionInBuffer += copied;
                read += length;
            }

            _Position += read;
            return read;
        }

        /// <inheritdoc/>
        [SkipLocalsInit]
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (!CanWrite)
                throw FileSystemExceptions.StreamReadOnly;

            // Don't initiate write if the buffer is empty
            if (buffer.IsEmpty)
                return;

            if (CanSeek && Position > Length)
            {
                // TODO: Maybe throw an exception?

                // Write gap
                var gapLength = Position - Length;

                // Generate weak noise
                var weakNoise = new byte[gapLength];
                Random.Shared.NextBytes(weakNoise);

                // Write contents of weak noise array
                WriteInternal(weakNoise, Length);
            }
            else
            {
                // Write contents
                WriteInternal(buffer, CanSeek ? Position : 0L);
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            if (!CanWrite)
                throw FileSystemExceptions.StreamReadOnly;

            if (!CanSeek)
                throw FileSystemExceptions.StreamNotSeekable;

            // Ignore resizing the same length
            if (value == Length)
                return;

            // Make sure header is ready before we can read/modify chunks
            if (!TryWriteHeader() && !TryReadHeader())
                throw new CryptographicException();

            var plaintextChunkSize = _security.ContentCrypt.ChunkPlaintextSize;

            // Determine whether to extend or truncate the file
            if (value < Length)
            {
                var remainingSize = (int)(value % plaintextChunkSize);
                if (remainingSize > 0)
                {
                    var lastChunkNumber = value / plaintextChunkSize;
                    _chunkAccess.SetChunkLength(lastChunkNumber, remainingSize);
                }

                // Update position to fit within new length
                _Position = Math.Min(value, _Position);
            }
            else if (value > Length)
            {
                // Flush remaining chunks before base stream is accessed
                _chunkAccess.Flush();

                // Calculate plaintext size here because plaintext Length might not be initialized yet
                var plaintextFileSize = _security.ContentCrypt.CalculatePlaintextSize(Inner.Length - _security.HeaderCrypt.HeaderCiphertextSize);
                var lastChunkNumber = plaintextFileSize / plaintextChunkSize;
                var amountToWrite = plaintextFileSize % plaintextChunkSize != 0 ? (int)Math.Min(plaintextChunkSize, value - plaintextFileSize) : 0;

                // Extend the chunk including already existing data
                _chunkAccess.SetChunkLength(lastChunkNumber, amountToWrite, true);
            }

            // Calculate ciphertext size based on new plaintext length
            var ciphertextFileSize = _security.HeaderCrypt.HeaderCiphertextSize + Math.Max(_security.ContentCrypt.CalculateCiphertextSize(value), 0L);

            // Flush modified chunks
            _chunkAccess.Flush();

            // Resize base stream to the ciphertext size
            Inner.SetLength(ciphertextFileSize);

            // Update plaintext length
            _Length = value;

            // Update last write time, if possible
            if (Inner is FileStream fileStream)
                File.SetLastWriteTime(fileStream.SafeFileHandle, DateTime.Now);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            if (!CanSeek)
                throw FileSystemExceptions.StreamNotSeekable;

            var seekPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };

            var ciphertextPosition = Math.Max(0L, AlignToChunkStartPosition(seekPosition));
            Inner.Position = ciphertextPosition;
            _Position = seekPosition;

            return Position;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            try
            {
                if (CanWrite)
                    Flush();
            }
            finally
            {
                base.Close();
                _notifyStreamClosed.Invoke(Inner);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            // Only flush when there's a need to
            if (_chunkAccess.FlushAvailable)
            {
                TryWriteHeader();
                _chunkAccess.Flush();
            }
        }

        private void WriteInternal(ReadOnlySpan<byte> buffer, long position)
        {
            if (!TryWriteHeader() && !TryReadHeader())
                throw new CryptographicException();

            var plaintextChunkSize = _security.ContentCrypt.ChunkPlaintextSize;
            var written = 0;
            var positionInBuffer = 0;

            while (positionInBuffer < buffer.Length)
            {
                var currentPosition = position + written;
                var chunkNumber = currentPosition / plaintextChunkSize;
                var offsetInChunk = (int)(currentPosition % plaintextChunkSize);
                var length = Math.Min(buffer.Length - positionInBuffer, plaintextChunkSize - offsetInChunk);

                var copy = _chunkAccess.CopyToChunk(
                    chunkNumber,
                    buffer.Slice(positionInBuffer),
                    (offsetInChunk == 0 && length == plaintextChunkSize) ? 0 : offsetInChunk);

                if (copy < 0)
                    throw new CryptographicException();

                positionInBuffer += copy;
                written += length;
            }

            if (CanSeek)
            {
                // Update length after writing
                _Length = Math.Max(position + written, Length);

                // Update position after writing
                _Position += written;
            }

            // Update last write time
            if (Inner is FileStream fileStream)
                File.SetLastWriteTime(fileStream.SafeFileHandle, DateTime.Now);
        }

        [SkipLocalsInit]
        private bool TryReadHeader()
        {
            if (!_headerBuffer.IsHeaderReady && CanRead && CanSeek)
            {
                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Read header
                var savedPos = Inner.Position;
                Inner.Position = 0L;
                var read = Inner.Read(ciphertextHeader);
                Inner.Position = savedPos;

                // Check if read is correct
                if (read < ciphertextHeader.Length)
                    return false;

                // Decrypt header
                _headerBuffer.IsHeaderReady = _security.HeaderCrypt.DecryptHeader(ciphertextHeader, _headerBuffer);
            }

            return _headerBuffer.IsHeaderReady;
        }

        [SkipLocalsInit]
        private bool TryWriteHeader()
        {
            if (!CanWrite)
                throw FileSystemExceptions.StreamReadOnly;

            lock (_writeLock)
                if (!_headerBuffer.IsHeaderReady && CanWrite)
                {
                    // Check if there is data already written only when we can seek
                    if (CanSeek && Inner.Length > 0L)
                        return false;

                    // Make sure we save the header state
                    _headerBuffer.IsHeaderReady = true;

                    // Allocate ciphertext header
                    Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                    // Get and encrypt header
                    _security.HeaderCrypt.CreateHeader(_headerBuffer);
                    _security.HeaderCrypt.EncryptHeader(_headerBuffer, ciphertextHeader);

                    // Write header
                    if (CanSeek)
                    {
                        var savedPos = Inner.Position;
                        Inner.Position = 0L;
                        Inner.Write(ciphertextHeader);
                        Inner.Position = savedPos + ciphertextHeader.Length;
                    }
                    else
                    {
                        Inner.Write(ciphertextHeader);
                    }

                    return true;
                }

            return false;
        }

        private long AlignToChunkStartPosition(long plaintextPosition)
        {
            var maxCiphertextPayloadSize = long.MaxValue - _security.HeaderCrypt.HeaderCiphertextSize;
            var maxChunks = maxCiphertextPayloadSize / _security.ContentCrypt.ChunkCiphertextSize;
            var chunkNumber = plaintextPosition / _security.ContentCrypt.ChunkPlaintextSize;

            if (chunkNumber > maxChunks)
                return long.MaxValue;

            return chunkNumber * _security.ContentCrypt.ChunkCiphertextSize + _security.HeaderCrypt.HeaderCiphertextSize;
        }
    }
}