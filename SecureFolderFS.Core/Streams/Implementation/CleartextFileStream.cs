using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;
using SecureFolderFS.Core.Chunks;
using SecureFolderFS.Core.Chunks.IO;
using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Extensions;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.InternalStreams;

namespace SecureFolderFS.Core.Streams.Implementation
{
    internal sealed class CleartextFileStream : Stream, ICleartextFileStream, IBaseFileStreamInternal, ICleartextFileStreamInternal
    {
        private readonly ISecurity _security;

        private readonly IFileSystemOperations _fileSystemOperations;

        private readonly IChunkFactory _chunkFactory;

        private readonly ICiphertextFileStream _ciphertextFileStream;

        private readonly IFileHeader _fileHeader;

        private readonly ICiphertextPath _ciphertextPath;

        private bool _isHeaderWritten;

        public IChunkReceiver ChunkReceiver { get; set; }

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
            set => this.Seek(value, SeekOrigin.Begin);
        }

        public override bool CanRead => _ciphertextFileStream.CanRead;

        public override bool CanSeek => _ciphertextFileStream.CanSeek;

        public override bool CanWrite => _ciphertextFileStream.CanWrite;

        public bool IsDisposed => _ciphertextFileStream.IsDisposed;

        public CleartextFileStream(
            ISecurity security,
            IFileSystemOperations fileSystemOperations,
            IChunkFactory chunkFactory,
            ICiphertextFileStream ciphertextFileStream,
            IFileHeader fileHeader,
            bool isHeaderWritten,
            
            ICiphertextPath ciphertextPath, FileMode mode, FileAccess access, FileShare share)
            //: base(ciphertextPath, mode, access, share)
        {
            this._security = security;
            this._fileSystemOperations = fileSystemOperations;
            this._chunkFactory = chunkFactory;
            this._ciphertextFileStream = ciphertextFileStream;
            this._fileHeader = fileHeader;
            this._isHeaderWritten = isHeaderWritten;
            this._ciphertextPath = ciphertextPath;

            this._Length = _security.ContentCryptor.FileContentCryptor.CalculateCleartextSize(_ciphertextFileStream.Length - _security.ContentCryptor.FileHeaderCryptor.HeaderSize);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            long oldFileSize = Length;
            if (Position > oldFileSize)
            {
                long gapLength = Position - oldFileSize;
                WriteToFillSpace(oldFileSize, gapLength);
            }
            else
            {
                using var memoryStreamBuffer = new MemoryStream(buffer, offset, Math.Min(count, buffer.Length - offset));
                WriteInternal(memoryStreamBuffer, Position);
            }

            _Position += count;
        }

        public override int Read([In, Out] byte[] buffer, int offset, int count)
        {
            ArgumentNullException.ThrowIfNull(buffer);

            int read = 0;
            long lengthToEof = _Length - _Position;
            if (lengthToEof < 1)
            {
                return Constants.IO.FILE_EOF;
            }

            int payloadSize = this._security.ContentCryptor.FileContentCryptor.ChunkCleartextSize;

            using (MemoryStream streamBuffer = new MemoryStream(buffer, offset, count))
            {
                long newLength = Math.Min(count, lengthToEof);
                streamBuffer.SetLength(newLength);

                while (streamBuffer.HasRemainingLength())
                {
                    long readPosition = Position + read;
                    long chunkNumber = readPosition / payloadSize;
                    int offsetInChunk = (int)(readPosition % payloadSize);
                    int length = (int)Math.Min(streamBuffer.RemainingLength(), payloadSize - offsetInChunk);

                    ICleartextChunk cleartextChunk = ChunkReceiver.GetChunk(chunkNumber);
                    cleartextChunk.CopyTo(streamBuffer, offsetInChunk);
                    read += length;
                }
            }

            _Position += read;
            return read;
        }

        public override void SetLength(long value)
        {
            if (value < _Length)
            {
                int cleartextChunkSize = _security.ContentCryptor.FileContentCryptor.ChunkCleartextSize;
                long numberOfLastChunk = (value + cleartextChunkSize - 1) / cleartextChunkSize - 1;
                int sizeOfIncompleteChunk = (int)(value % cleartextChunkSize);

                if (sizeOfIncompleteChunk > 0)
                {
                    ICleartextChunk cleartextChunk = ChunkReceiver.GetChunk(numberOfLastChunk);
                    cleartextChunk.SetActualLength(sizeOfIncompleteChunk);
                }

                long ciphertextFileSize = _security.ContentCryptor.FileHeaderCryptor.HeaderSize + _security.ContentCryptor.FileContentCryptor.CalculateCiphertextSize(value);
                ChunkReceiver.Flush();
                _ciphertextFileStream.SetLength(ciphertextFileSize);
                _Length = value;
                _Position = Math.Min(value, Position);
                _fileSystemOperations.DangerousFileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
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
            this._Position = positionToSeek;

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
                ChunkReceiver.Flush();
            }
        }

        private void WriteInternal(MemoryStream streamBuffer, long position)
        {
            TryWriteHeader();

            var cleartextChunkSize = this._security.ContentCryptor.FileContentCryptor.ChunkCleartextSize;
            var written = 0;

            while (streamBuffer.HasRemainingLength())
            {
                long currentPosition = position + written;
                long chunkNumber = currentPosition / cleartextChunkSize;
                int offsetInChunk = (int)(currentPosition % cleartextChunkSize);
                int length = (int)Math.Min(streamBuffer.RemainingLength(), cleartextChunkSize - offsetInChunk);

                ICleartextChunk cleartextChunk;
                if (offsetInChunk == 0 && length == cleartextChunkSize)
                {
                    cleartextChunk = _chunkFactory.FromCleartextChunkBuffer(new byte[cleartextChunkSize], 0);
                    cleartextChunk.CopyFrom(streamBuffer, 0);
                    ChunkReceiver.SetChunk(chunkNumber, cleartextChunk);
                }
                else
                {
                    cleartextChunk = ChunkReceiver.GetChunk(chunkNumber);
                    cleartextChunk.CopyFrom(streamBuffer, offsetInChunk);
                }

                written += length;
            }

            _Length = Math.Max(position + written, Length);
            _fileSystemOperations.DangerousFileOperations.SetLastWriteTime(_ciphertextPath.Path, DateTime.Now);
        }

        public bool CanBeDeleted()
        {
            if (PlatformDataModel.IsPlatformOSX)
            {
                return true;
            }

            try
            {
                _ciphertextFileStream.Lock(0, _ciphertextFileStream.Length);
            }
            catch (IOException)
            {
                // The file is unavailable because it is:
                // still being written to
                // or being processed by another thread
                // or does not exist
                return false;
            }
            finally
            {
                try
                {
                    _ciphertextFileStream.Unlock(0, Length);
                }
                catch
                {
                    // Catch any errors caused by Unlock() (if Lock() had failed).
                }
            }

            // File is not locked
            return true;
        }

        public void Lock(long position, long length)
        {
            _ciphertextFileStream.Lock(position, length);
        }

        public void Unlock(long position, long length)
        {
            _ciphertextFileStream.Unlock(position, length);
        }

        public SafeFileHandle GetSafeFileHandle()
        {
            return _ciphertextFileStream.AsBaseFileStreamInternal().GetSafeFileHandle();
        }

        public ICiphertextFileStream GetInternalCiphertextFileStream()
        {
            return _ciphertextFileStream;
        }

        private void WriteToFillSpace(long position, long count)
        {
            using var weakNoiseMemoryStream = new MemoryStream(ArrayExtensions.GenerateWeakNoise(count));

            WriteInternal(weakNoiseMemoryStream, position);
        }

        private bool TryWriteHeader()
        {
            if (!_isHeaderWritten && CanWrite)
            {
                _isHeaderWritten = true;

                byte[] headerBuffer = _security.ContentCryptor.FileHeaderCryptor.EncryptHeader(_fileHeader);
                long savedPosition = _ciphertextFileStream.Position;
                _ciphertextFileStream.Position = 0L;
                _ciphertextFileStream.Write(headerBuffer, 0, headerBuffer.Length);
                _ciphertextFileStream.Position = savedPosition + headerBuffer.Length;

                return true;
            }

            return false;
        }

        private long BeginOfChunk(long cleartextPosition)
        {
            long maxCiphertextPayloadSize = long.MaxValue - _security.ContentCryptor.FileHeaderCryptor.HeaderSize;
            long maxChunks = maxCiphertextPayloadSize / _security.ContentCryptor.FileContentCryptor.ChunkFullCiphertextSize;
            long chunk = cleartextPosition / _security.ContentCryptor.FileContentCryptor.ChunkCleartextSize;

            if (chunk > maxChunks)
            {
                return long.MaxValue;
            }
            else
            {
                return chunk * _security.ContentCryptor.FileContentCryptor.ChunkFullCiphertextSize + _security.ContentCryptor.FileHeaderCryptor.HeaderSize;
            }
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
