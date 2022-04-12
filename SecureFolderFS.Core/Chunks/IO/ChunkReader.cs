using System;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Streams.Management;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal sealed class ChunkReader : IChunkReader
    {
        private readonly ISecurity _security;

        private readonly IFileHeader _fileHeader;

        private readonly IChunkFactory _chunkFactory;

        private readonly CiphertextStreamsManager _ciphertextStreamsManager;

        private readonly IFileSystemStatsTracker _fileSystemStatsTracker;

        private bool _disposed;

        public ChunkReader(ISecurity security, IFileHeader fileHeader, IChunkFactory chunkFactory, CiphertextStreamsManager ciphertextStreamsManager, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            this._security = security;
            this._fileHeader = fileHeader;
            this._chunkFactory = chunkFactory;
            this._ciphertextStreamsManager = ciphertextStreamsManager;
            this._fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public ICleartextChunk ReadChunk(long chunkNumber)
        {
            AssertNotDisposed();

            var payloadSize = _security.ContentCryptor.FileContentCryptor.ChunkCleartextSize;
            var chunkSize = _security.ContentCryptor.FileContentCryptor.ChunkFullCiphertextSize;

            var ciphertextPosition = _security.ContentCryptor.FileHeaderCryptor.HeaderSize + (chunkNumber * chunkSize);
            var ciphertextBuffer = new byte[chunkSize];

            var ciphertextFileStream = _ciphertextStreamsManager.EnsureReadOnlyStreamInstance();
            ciphertextFileStream.Position = ciphertextPosition;
            var read = ciphertextFileStream.Read(ciphertextBuffer, 0, ciphertextBuffer.Length);

            if (read == Constants.IO.FILE_EOF)
            {
                return _chunkFactory.FromCleartextChunkBuffer(new byte[payloadSize], 0);
            }

            _fileSystemStatsTracker?.AddBytesRead(read);

            var actualCiphertextBuffer = new byte[read];
            actualCiphertextBuffer.EmplaceArrays(ciphertextBuffer);

            var cleartextChunk = _security.ContentCryptor.FileContentCryptor.DecryptChunk(
                _chunkFactory.FromCiphertextChunkBuffer(actualCiphertextBuffer),
                chunkNumber,
                _fileHeader,
                Constants.Security.ALWAYS_CHECK_CHUNK_INTEGRITY);

            _fileSystemStatsTracker?.AddBytesDecrypted(cleartextChunk.ActualLength);

            return cleartextChunk;
        }

        private void AssertNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }

        public void Dispose()
        {
            _disposed = true;
            _fileHeader?.Dispose();
            _ciphertextStreamsManager?.Dispose();
        }
    }
}
