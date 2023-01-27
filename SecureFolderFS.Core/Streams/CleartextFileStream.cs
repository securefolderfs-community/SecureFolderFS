using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Streams
{
    /// <inheritdoc cref="CleartextStream"/>
    internal sealed class CleartextFileStream : CleartextStream
    {
        private readonly Security _security;
        private readonly IChunkAccess _chunkAccess;
        private readonly HeaderBuffer _headerBuffer;
        private readonly Action<Stream> _notifyStreamClosed;

        private long _Length;
        private long _Position;

        /// <inheritdoc/>
        public override long Position
        {
            get => _Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override bool CanRead => Inner.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => Inner.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => Inner.CanWrite;

        public CleartextFileStream(
            Stream ciphertextStream,
            Security security,
            IChunkAccess chunkAccess,
            HeaderBuffer headerBuffer,
            Action<Stream> notifyStreamClosed)
            : base(ciphertextStream)
        {
            _security = security;
            _chunkAccess = chunkAccess;
            _headerBuffer = headerBuffer;
            _notifyStreamClosed = notifyStreamClosed;
            _Length = Math.Max(_security.ContentCrypt.CalculateCleartextSize(ciphertextStream.Length - _security.HeaderCrypt.HeaderCiphertextSize), 0L);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            var ciphertextStreamLength = Inner.Length;

            if (ciphertextStreamLength == 0L)
                return FileSystem.Constants.FILE_EOF;

            if (ciphertextStreamLength < _security.HeaderCrypt.HeaderCiphertextSize)
                return FileSystem.Constants.FILE_EOF; // TODO: HealthAPI - report invalid header size

            var lengthToEof = Length - _Position;
            if (lengthToEof < 1L)
                return FileSystem.Constants.FILE_EOF;

            // Read header if not ready
            if (!TryReadHeader())
                throw new CryptographicException();

            var read = 0;
            var positionInBuffer = 0;
            var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
            var adjustedBuffer = buffer.Slice(0, (int)Math.Min(buffer.Length, lengthToEof));

            while (positionInBuffer < adjustedBuffer.Length)
            {
                var readPosition = _Position + read;
                var chunkNumber = readPosition / cleartextChunkSize;
                var offsetInChunk = (int)(readPosition % cleartextChunkSize);
                var length = Math.Min(adjustedBuffer.Length - positionInBuffer, cleartextChunkSize - offsetInChunk);

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
            // Don't initiate write if the buffer is empty
            if (buffer.IsEmpty)
                return;

            if (Position > Length)
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
                WriteInternal(buffer, Position);
            }
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            // Make sure header is ready before we can read/modify chunks
            if (!TryWriteHeader(false))
                throw new CryptographicException();

            // Ignore resizing the same length
            if (value == Length)
                return;

            var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;

            // Determine whether to extend or truncate the file
            if (value < Length)
            {
                var remainingSize = (int)(value % cleartextChunkSize);
                if (remainingSize > 0)
                {
                    var lastChunkNumber = value / cleartextChunkSize;
                    _chunkAccess.SetChunkLength(lastChunkNumber, remainingSize);
                }

                // Update position to fit within new length
                _Position = Math.Min(value, _Position);
            }
            else if (value > Length)
            {
                // Flush remaining chunks before base stream is accessed
                _chunkAccess.Flush();

                // Calculate cleartext size here because cleartext Length might not be initialized yet
                var cleartextFileSize = _security.ContentCrypt.CalculateCleartextSize(Inner.Length - _security.HeaderCrypt.HeaderCiphertextSize);
                var lastChunkNumber = cleartextFileSize / cleartextChunkSize;
                var amountToWrite = cleartextFileSize % cleartextChunkSize != 0 ? (int)Math.Min(cleartextChunkSize, value - cleartextFileSize) : 0;

                // Extend the chunk including already existing data
                _chunkAccess.SetChunkLength(lastChunkNumber, amountToWrite, true);
            }

            // Calculate ciphertext size based on new cleartext length
            var ciphertextFileSize = _security.HeaderCrypt.HeaderCiphertextSize + Math.Max(_security.ContentCrypt.CalculateCiphertextSize(value), 0L);

            // Flush modified chunks
            _chunkAccess.Flush();

            // Resize base stream to the ciphertext size
            Inner.SetLength(ciphertextFileSize);

            // Update cleartext length
            _Length = value;

            // Update last write time, if possible
            if (Inner is FileStream fileStream)
                File.SetLastWriteTime(fileStream.SafeFileHandle, DateTime.Now);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var seekPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => throw new ArgumentOutOfRangeException(nameof(origin))
            };

            var ciphertextPosition = AlignToChunkStartPosition(seekPosition);
            Inner.Position = ciphertextPosition;
            _Position = seekPosition;

            return Position;
        }

        /// <inheritdoc/>
        public override void Close()
        {
            try
            {
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
            if (!CanWrite)
                return;

            TryWriteHeader(true);
            _chunkAccess.Flush();
        }

        private void WriteInternal(ReadOnlySpan<byte> buffer, long position)
        {
            if (!TryWriteHeader(false))
                throw new CryptographicException();

            var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
            var written = 0;
            var positionInBuffer = 0;

            while (positionInBuffer < buffer.Length)
            {
                var currentPosition = position + written;
                var chunkNumber = currentPosition / cleartextChunkSize;
                var offsetInChunk = (int)(currentPosition % cleartextChunkSize);
                var length = Math.Min(buffer.Length - positionInBuffer, cleartextChunkSize - offsetInChunk);

                var copy = _chunkAccess.CopyToChunk(
                    chunkNumber,
                    buffer.Slice(positionInBuffer), (offsetInChunk == 0 && length == cleartextChunkSize) ? 0 : offsetInChunk);

                if (copy < 0)
                    throw new CryptographicException();

                positionInBuffer += copy;
                written += length;
            }

            // Update length after writing
            _Length = Math.Max(position + written, Length);

            // Update position after writing
            _Position += written;

            // Update last write time
            if (Inner is FileStream fileStream)
                File.SetLastWriteTime(fileStream.SafeFileHandle, DateTime.Now);
        }

        [SkipLocalsInit]
        private bool TryReadHeader()
        {
            if (!_headerBuffer.IsHeaderReady && CanRead && Inner.Length >= _security.HeaderCrypt.HeaderCiphertextSize)
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
        private bool TryWriteHeader(bool skipRead)
        {
            if (!_headerBuffer.IsHeaderReady && CanWrite && Inner.Length == 0L)
            {
                // Make sure we save the header state
                _headerBuffer.IsHeaderReady = true;

                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Get and encrypt header
                _security.HeaderCrypt.CreateHeader(_headerBuffer);
                _security.HeaderCrypt.EncryptHeader(_headerBuffer, ciphertextHeader);

                // Write header
                var savedPos = Inner.Position;
                Inner.Position = 0L;
                Inner.Write(ciphertextHeader);
                Inner.Position = savedPos + ciphertextHeader.Length;

                return true;
            }

            return skipRead || TryReadHeader();
        }

        private long AlignToChunkStartPosition(long cleartextPosition)
        {
            var maxCiphertextPayloadSize = long.MaxValue - _security.HeaderCrypt.HeaderCiphertextSize;
            var maxChunks = maxCiphertextPayloadSize / _security.ContentCrypt.ChunkCiphertextSize;
            var chunkNumber = cleartextPosition / _security.ContentCrypt.ChunkCleartextSize;

            if (chunkNumber > maxChunks)
                return long.MaxValue;

            return chunkNumber * _security.ContentCrypt.ChunkCiphertextSize + _security.HeaderCrypt.HeaderCiphertextSize;
        }
    }
}