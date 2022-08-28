using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.Streams
{
    internal sealed class CleartextFileStream : Stream, ICleartextFileStreamEx
    {
        private readonly ISecurity _security;
        private readonly IChunkAccess _chunkAccess;
        private readonly Stream _ciphertextStream;
        private readonly CleartextHeaderBuffer _fileHeader;

        private long _Length;
        private long _Position;

        public Stream CiphertextStream => _ciphertextStream;

        public Action<CleartextFileStream>? StreamClosedCallback { get; set; }

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
            Stream ciphertextStream,
            CleartextHeaderBuffer fileHeader)
        {
            _security = security;
            _chunkAccess = chunkAccess;
            _ciphertextStream = ciphertextStream;
            _fileHeader = fileHeader;
            _Length = Math.Max(_security.ContentCrypt.CalculateCleartextSize(ciphertextStream.Length - _security.HeaderCrypt.HeaderCiphertextSize), 0L);
        }

        /// <inheritdoc/>
        public override int Read(Span<byte> buffer)
        {
            var ciphertextStreamLength = _ciphertextStream.Length;

            if (ciphertextStreamLength == 0L)
                return Constants.IO.FILE_EOF;

            if (ciphertextStreamLength < _security.HeaderCrypt.HeaderCiphertextSize)
                return Constants.IO.FILE_EOF; // TODO: Health - report invalid header size

            var lengthToEof = _Length - _Position;
            if (lengthToEof < 1L)
                return Constants.IO.FILE_EOF;

            // Read header if not ready
            if (!TryReadHeader())
                throw new UnauthenticFileHeaderException();

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
                    throw new UnauthenticChunkException();

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
                StreamClosedCallback?.Invoke(this);
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
        public void Lock(long position, long length)
        {
            (_ciphertextStream as FileStream)?.Lock(position, length);
        }

        /// <inheritdoc/>
        public void Unlock(long position, long length)
        {
            (_ciphertextStream as FileStream)?.Unlock(position, length);
        }

        #region Unused

        /// <inheritdoc/>
        public override int Read(byte[] buffer, int offset, int count) =>
            this.Read(buffer.AsSpan(offset, Math.Min(count, buffer.Length - offset)));

        /// <inheritdoc/>
        public override void Write(byte[] buffer, int offset, int count) =>
            this.Write(buffer.AsSpan(offset, Math.Min(count, buffer.Length - offset)));
        
        #endregion

        private void WriteInternal(ReadOnlySpan<byte> buffer, long position)
        {
            if (!TryWriteHeader(false))
                throw new UnauthenticFileHeaderException();

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
                    throw new UnauthenticChunkException();

                positionInBuffer += copy;
                written += length;
            }

            _Length = Math.Max(position + written, Length);
            //_fileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
        }

        [SkipLocalsInit]
        private bool TryReadHeader()
        {
            if (!_fileHeader.IsHeaderReady && CanRead && _ciphertextStream.Length >= _security.HeaderCrypt.HeaderCiphertextSize)
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
                _fileHeader.IsHeaderReady = _security.HeaderCrypt.DecryptHeader(ciphertextHeader, _fileHeader);
            }

            return _fileHeader.IsHeaderReady;
        }

        [SkipLocalsInit]
        private bool TryWriteHeader(bool skipRead)
        {
            if (!_fileHeader.IsHeaderReady && CanWrite && _ciphertextStream.Length == 0L)
            {
                // Make sure we save the header state
                _fileHeader.IsHeaderReady = true;

                // Allocate ciphertext header
                Span<byte> ciphertextHeader = stackalloc byte[_security.HeaderCrypt.HeaderCiphertextSize];

                // Get and encrypt header
                _security.HeaderCrypt.CreateHeader(_fileHeader);
                _security.HeaderCrypt.EncryptHeader(_fileHeader, ciphertextHeader);

                // Write header
                var savedPos = _ciphertextStream.Position;
                _ciphertextStream.Position = 0L;
                _ciphertextStream.Write(ciphertextHeader);
                _ciphertextStream.Position = savedPos + ciphertextHeader.Length;

                return true;
            }

            if (!skipRead)
                return TryReadHeader();

            return _fileHeader.IsHeaderReady;
        }

        private long BeginOfChunk(long cleartextPosition)
        {
            var maxCiphertextPayloadSize = long.MaxValue - _security.HeaderCrypt.HeaderCiphertextSize;
            var maxChunks = maxCiphertextPayloadSize / _security.ContentCrypt.ChunkCiphertextSize;
            var chunk = cleartextPosition / _security.ContentCrypt.ChunkCleartextSize;

            if (chunk > maxChunks)
            {
                return long.MaxValue;
            }
            else
            {
                return chunk * _security.ContentCrypt.ChunkCiphertextSize + _security.HeaderCrypt.HeaderCiphertextSize;
            }
        }
    }
}
