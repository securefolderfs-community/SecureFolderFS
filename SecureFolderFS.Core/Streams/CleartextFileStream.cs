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
        public override bool CanRead => BaseStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => BaseStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => BaseStream.CanWrite;

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
            var ciphertextStreamLength = BaseStream.Length;

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
            if (!TryWriteHeader(false))
                throw new CryptographicException();

            if (value == Length)
                return;

            if (value < Length)
            {
                var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
                var missingSize = (int)(value % cleartextChunkSize);

                if (missingSize > 0)
                {
                    var lastChunkNumber = ((value + cleartextChunkSize - 1) / cleartextChunkSize) - 1;
                    _chunkAccess.SetChunkLength(lastChunkNumber, missingSize);
                }

                _Position = Math.Min(value, _Position);
            }
            else if (value > Length)
            {
                var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
                var amountToWrite = value - Length;
                var iterationSize = (int)Math.Min(cleartextChunkSize, amountToWrite); // Can cast to int, because cleartextChunkSize is int
                var numberOfPasses = Math.Max(1, Math.Ceiling(amountToWrite / (float)iterationSize));

                var written = 0L;
                var emptyBytes = new byte[iterationSize].AsSpan();
                var savedPosition = _Position;
                _Position = Length;

                for (var i = 0L; i < numberOfPasses; i++)
                {
                    var remaining = (int)Math.Min(iterationSize, amountToWrite - written); // Can cast to int, because iterationSize is int
                    Write(emptyBytes.Slice(0, remaining));

                    written += remaining;
                }

                _Position = savedPosition;
            }

            var ciphertextFileSize = _security.HeaderCrypt.HeaderCiphertextSize + Math.Max(_security.ContentCrypt.CalculateCiphertextSize(value), 0L);
            _chunkAccess.Flush();
            BaseStream.SetLength(ciphertextFileSize);
            _Length = value;

            // Update last write time
            if (BaseStream is FileStream fileStream)
                File.SetLastWriteTime(fileStream.SafeFileHandle, DateTime.Now);
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var seekPosition = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.End => Length + offset,
                _ => _Position + offset
            };

            var ciphertextPosition = AlignToChunkStartPosition(seekPosition);
            BaseStream.Position = ciphertextPosition;
            _Position = seekPosition;

            return _Position;
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
                _notifyStreamClosed.Invoke(BaseStream);
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
            if (BaseStream is FileStream fileStream)
                File.SetLastWriteTime(fileStream.SafeFileHandle, DateTime.Now);
        }

        [SkipLocalsInit]
        private bool TryReadHeader()
        {
            if (!_headerBuffer.IsHeaderReady && CanRead && BaseStream.Length >= _security.HeaderCrypt.HeaderCiphertextSize)
            {
                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Read header
                var savedPos = BaseStream.Position;
                BaseStream.Position = 0L;
                var read = BaseStream.Read(ciphertextHeader);
                BaseStream.Position = savedPos;

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
            if (!_headerBuffer.IsHeaderReady && CanWrite && BaseStream.Length == 0L)
            {
                // Make sure we save the header state
                _headerBuffer.IsHeaderReady = true;

                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Get and encrypt header
                _security.HeaderCrypt.CreateHeader(_headerBuffer);
                _security.HeaderCrypt.EncryptHeader(_headerBuffer, ciphertextHeader);

                // Write header
                var savedPos = BaseStream.Position;
                BaseStream.Position = 0L;
                BaseStream.Write(ciphertextHeader);
                BaseStream.Position = savedPos + ciphertextHeader.Length;

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
