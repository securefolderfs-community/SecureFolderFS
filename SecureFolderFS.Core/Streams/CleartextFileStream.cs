using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Extensions;
using SecureFolderFS.Core.Sdk.Streams;
using SecureFolderFS.Core.Security;
using System;
using System.Diagnostics;
using System.IO;

namespace SecureFolderFS.Core.Streams
{
    internal sealed class CleartextFileStream : Stream, ICleartextFileStream
    {
        private readonly ISecurity _security;
        private readonly IChunkAccess _chunkAccess;
        private readonly ICiphertextFileStream _ciphertextFileStream;
        private readonly CleartextHeaderBuffer _fileHeader;

        // TODO: Remove?
        private bool _isHeaderWritten;

        public ICiphertextFileStream UnderlyingStream { get; }

        public Action<ICleartextFileStream> StreamClosedCallback { get; set; }

        private long _Length;
        public override long Length
        {
            get => _Length;
        }

        private long _Position;
        public override long Position 
        {
            get => _Position;
            set => Seek(value, SeekOrigin.Begin);
        }

        public override bool CanRead => _ciphertextFileStream.CanRead;

        public override bool CanSeek => _ciphertextFileStream.CanSeek;

        public override bool CanWrite => _ciphertextFileStream.CanWrite;

        public bool IsDisposed => _ciphertextFileStream.IsDisposed;

        public CleartextFileStream(
            ISecurity security,
            IChunkAccess chunkAccess,
            ICiphertextFileStream ciphertextFileStream,
            CleartextHeaderBuffer fileHeader,
            bool isHeaderWritten)
            //: base(ciphertextPath, mode, access, share)
        {
            _security = security;
            _chunkAccess = chunkAccess;
            _ciphertextFileStream = ciphertextFileStream;
            _fileHeader = fileHeader;
            _isHeaderWritten = isHeaderWritten;
            _Length = _security.ContentCrypt.CalculateCleartextSize(_ciphertextFileStream.Length - _security.HeaderCrypt.HeaderCiphertextSize);
            _Length = _Length == -1 ? 0 : _Length;

            UnderlyingStream = ciphertextFileStream;
        }

        public override int Read(Span<byte> buffer)
        {
            var lengthToEof = _Length - _Position;
            if (lengthToEof < 1L)
                return Constants.IO.FILE_EOF;

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

                positionInBuffer += _chunkAccess.CopyFromChunk(chunkNumber, adjustedBuffer.Slice(positionInBuffer), offsetInChunk);
                read += length;
            }

            _Position += read;
            return read;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            return Read(buffer.AsSpan(offset, Math.Min(count, buffer.Length - offset)));
        }

        public override void Write(ReadOnlySpan<byte> buffer)
        {
            if (buffer.IsEmpty)
                return;

            if (Position > Length)
            {
                Debugger.Break(); // Write gap
                //var gapLength = Position - oldFileSize;
                //WriteInternal(ArrayExtensions.GenerateWeakNoise(oldFileSize), gapLength);
            }
            else
            {
                WriteInternal(buffer, Position);
            }

            _Position += buffer.Length;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            Write(buffer.AsSpan(offset, Math.Min(count, buffer.Length - offset)));
        }

        public override void SetLength(long value)
        {
            if (value < _Length)
            {
                int cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
                long numberOfLastChunk = (value + cleartextChunkSize - 1) / cleartextChunkSize - 1;
                int sizeOfIncompleteChunk = (int)(value % cleartextChunkSize);

                if (sizeOfIncompleteChunk > 0)
                {
                    Debugger.Break();
                    //ICleartextChunk cleartextChunk = ChunkReceiver.GetChunk(numberOfLastChunk);
                    //cleartextChunk.SetActualLength(sizeOfIncompleteChunk);
                }

                long ciphertextFileSize = _security.HeaderCrypt.HeaderCiphertextSize + _security.ContentCrypt.CalculateCiphertextSize(value);
                _chunkAccess.Flush();
                _ciphertextFileStream.SetLength(ciphertextFileSize);
                _Length = value;
                _Position = Math.Min(value, Position);
                //_fileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
            }
            else
            {
                return;
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long positionToSeek;
            if (origin == SeekOrigin.Begin)
            {
                positionToSeek = 0 + offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                positionToSeek = _Position + offset;
            }
            else if (origin == SeekOrigin.End)
            {
                positionToSeek = Length + offset;
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(origin));
            }

            long actualPositionToSeek = BeginOfChunk(positionToSeek);
            _ciphertextFileStream.Position = actualPositionToSeek;
            _Position = positionToSeek;

            return _Position;
        }

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

        public override void Flush()
        {
            if (CanWrite)
            {
                TryWriteHeader();
                _chunkAccess.Flush();
            }
        }

        private void WriteInternal(ReadOnlySpan<byte> buffer, long position)
        {
            TryWriteHeader();

            var cleartextChunkSize = _security.ContentCrypt.ChunkCleartextSize;
            var written = 0;
            var positionInBuffer = 0;

            while (positionInBuffer < buffer.Length)
            {
                var currentPosition = position + written;
                var chunkNumber = currentPosition / cleartextChunkSize;
                var offsetInChunk = (int)(currentPosition % cleartextChunkSize);
                var length = Math.Min(buffer.Length - positionInBuffer, cleartextChunkSize - offsetInChunk);

                if (offsetInChunk == 0 && length == cleartextChunkSize)
                {
                    positionInBuffer += _chunkAccess.CopyToChunk(chunkNumber, buffer.Slice(positionInBuffer), 0);
                }
                else
                {
                    positionInBuffer += _chunkAccess.CopyToChunk(chunkNumber, buffer.Slice(positionInBuffer), offsetInChunk);
                }

                written += length;
            }

            _Length = Math.Max(position + written, Length);
            //_fileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
        }

        public void Lock(long position, long length)
        {
            _ciphertextFileStream.Lock(position, length);
        }

        public void Unlock(long position, long length)
        {
            _ciphertextFileStream.Unlock(position, length);
        }

        private void TryWriteHeader()
        {
            if (!_isHeaderWritten && CanWrite)
            {
                _isHeaderWritten = true;

                var headerBuffer = new byte[_security.HeaderCrypt.HeaderCiphertextSize];

                _security.HeaderCrypt.EncryptHeader(_fileHeader, headerBuffer);
                var savedPosition = _ciphertextFileStream.Position;
                _ciphertextFileStream.Position = 0L;
                _ciphertextFileStream.Write(headerBuffer.AsSpan());
                _ciphertextFileStream.Position = savedPosition + headerBuffer.Length;
            }
        }

        private long BeginOfChunk(long cleartextPosition)
        {
            long maxCiphertextPayloadSize = long.MaxValue - _security.HeaderCrypt.HeaderCiphertextSize;
            long maxChunks = maxCiphertextPayloadSize / _security.ContentCrypt.ChunkCiphertextSize;
            long chunk = cleartextPosition / _security.ContentCrypt.ChunkCleartextSize;

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
