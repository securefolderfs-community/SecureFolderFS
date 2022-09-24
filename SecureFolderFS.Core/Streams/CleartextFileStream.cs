using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.Streams
{
    internal sealed class CleartextFileStream : CleartextStream
    {
        private readonly ISecurity _security;
        private readonly IChunkAccess _chunkAccess;
        private readonly HeaderBuffer _headerBuffer;
        private readonly Stream _ciphertextStream;
        private readonly Action<Stream> _notifyStreamClosed;

        private long _Length;
        private long _Position;

        public Stream CiphertextStream => _ciphertextStream;

        /// <inheritdoc/>
        public override long Position 
        {
            get => _Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        /// <inheritdoc/>
        public override long Length => _Length;

        /// <inheritdoc/>
        public override bool CanRead => _ciphertextStream.CanRead;

        /// <inheritdoc/>
        public override bool CanSeek => _ciphertextStream.CanSeek;

        /// <inheritdoc/>
        public override bool CanWrite => _ciphertextStream.CanWrite;

        public CleartextFileStream(
            ISecurity security,
            IChunkAccess chunkAccess,
            HeaderBuffer headerBuffer,
            Stream ciphertextStream,
            Action<Stream> notifyStreamClosed)
        {
            _security = security;
            _chunkAccess = chunkAccess;
            _headerBuffer = headerBuffer;
            _ciphertextStream = ciphertextStream;
            _notifyStreamClosed = notifyStreamClosed;
            _Length = Math.Max(_security.ContentCrypt.CalculateCleartextSize(ciphertextStream.Length - _security.HeaderCrypt.HeaderCiphertextSize), 0L);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            var ciphertextStreamLength = _ciphertextStream.Length;

            if (ciphertextStreamLength == 0L)
                return FileSystem.Constants.FILE_EOF;

            if (ciphertextStreamLength < _security.HeaderCrypt.HeaderCiphertextSize)
                return FileSystem.Constants.FILE_EOF; // TODO: Health - report invalid header size

            var lengthToEof = _Length - _Position;
            if (lengthToEof < 1L)
                return FileSystem.Constants.FILE_EOF;

            // Read header if not ready
            if (!TryReadHeader())
                throw new CryptographicException();

            var read = 0;
            var positionInBuffer = 0;
            var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
            var adjustedBuffer = buffer.Slice(0, Math.Min(buffer.Length, (int)lengthToEof));

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
        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IsEmpty)
                return;

            if (Position > Length)
            {
                // Write gap
                var gapLength = Position - Length;
                WriteInternal(ArrayExtensions.GenerateWeakNoise(gapLength), Length);
            }
            else
            {
                // Write contents
                WriteInternal(buffer, Position);
            }

            _Position += buffer.Length;
        }

        /// <inheritdoc/>
        public override void SetLength(long value)
        {
            if (value < _Length)
            {
                var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
                var numberOfLastChunk = (value + cleartextChunkSize - 1) / cleartextChunkSize - 1;
                var sizeOfIncompleteChunk = (int)(value % cleartextChunkSize);

                if (sizeOfIncompleteChunk > 0)
                {
                    Debugger.Break();
                    _chunkAccess.SetChunkLength(numberOfLastChunk, sizeOfIncompleteChunk);
                }

                var ciphertextFileSize = _security.HeaderCrypt.HeaderCiphertextSize + Math.Max(_security.ContentCrypt.CalculateCiphertextSize(value), 0L);
                _chunkAccess.Flush();
                _ciphertextStream.SetLength(ciphertextFileSize);
                _Length = value;
                _Position = Math.Min(value, Position);
                //_fileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
            }
        }

        /// <inheritdoc/>
        public override long Seek(long offset, SeekOrigin origin)
        {
            var seekPos = origin switch
            {
                SeekOrigin.Begin => offset,
                SeekOrigin.End => Length + offset,
                _ => _Position + offset
            };

            var realSeekPos = BeginOfChunk(seekPos);
            _ciphertextStream.Position = realSeekPos;
            _Position = seekPos;
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
                _notifyStreamClosed.Invoke(_ciphertextStream);
            }
        }

        /// <inheritdoc/>
        public override void Flush()
        {
            if (CanWrite)
            {
                TryWriteHeader(true);
                _chunkAccess.Flush();
            }
        }

        /// <inheritdoc/>
        public override void Lock(long position, long length)
        {
            (_ciphertextStream as FileStream)?.Lock(position, length);
        }

        /// <inheritdoc/>
        public override void Unlock(long position, long length)
        {
            (_ciphertextStream as FileStream)?.Unlock(position, length);
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

                int copy;
                if (offsetInChunk == 0 && length == cleartextChunkSize)
                {
                    copy = _chunkAccess.CopyToChunk(chunkNumber, buffer.Slice(positionInBuffer), 0);
                }
                else
                {
                    copy = _chunkAccess.CopyToChunk(chunkNumber, buffer.Slice(positionInBuffer), offsetInChunk);
                }

                if (copy < 0)
                    throw new CryptographicException();

                positionInBuffer += copy;
                written += length;
            }

            _Length = Math.Max(position + written, Length);
            //_fileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
        }

        [SkipLocalsInit]
        private bool TryReadHeader()
        {
            if (!_headerBuffer.IsHeaderReady && CanRead && _ciphertextStream.Length >= _security.HeaderCrypt.HeaderCiphertextSize)
            {
                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Read header
                var savedPos = _ciphertextStream.Position;
                _ciphertextStream.Position = 0L;
                var read = _ciphertextStream.Read(ciphertextHeader);
                _ciphertextStream.Position = savedPos;

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
            if (!_headerBuffer.IsHeaderReady && CanWrite && _ciphertextStream.Length == 0L)
            {
                // Make sure we save the header state
                _headerBuffer.IsHeaderReady = true;

                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Get and encrypt header
                _security.HeaderCrypt.CreateHeader(_headerBuffer);
                _security.HeaderCrypt.EncryptHeader(_headerBuffer, ciphertextHeader);

                // Write header
                var savedPos = _ciphertextStream.Position;
                _ciphertextStream.Position = 0L;
                _ciphertextStream.Write(ciphertextHeader);
                _ciphertextStream.Position = savedPos + ciphertextHeader.Length;

                return true;
            }
            
            return TryReadHeader();
        }

        private long BeginOfChunk(long cleartextPosition)
        {
            var maxCiphertextPayloadSize = long.MaxValue - _security.HeaderCrypt.HeaderCiphertextSize;
            var maxChunks = maxCiphertextPayloadSize / _security.ContentCrypt.ChunkCiphertextSize;
            var chunk = cleartextPosition / _security.ContentCrypt.ChunkCleartextSize;

            if (chunk > maxChunks)
                return long.MaxValue;

            return chunk * _security.ContentCrypt.ChunkCiphertextSize + _security.HeaderCrypt.HeaderCiphertextSize;
        }
    }
}
