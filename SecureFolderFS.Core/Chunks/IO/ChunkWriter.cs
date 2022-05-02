using System;
using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using SecureFolderFS.Sdk.Tracking;

namespace SecureFolderFS.Core.Chunks.IO
{
    internal sealed class ChunkWriter : IChunkWriter
    {
        private readonly ISecurity _security;

        private readonly IFileHeader _fileHeader;

        private readonly CiphertextStreamsManager _ciphertextStreamsManager;

        private readonly IFileSystemStatsTracker _fileSystemStatsTracker;

        private bool _disposed;

        public ChunkWriter(ISecurity security, IFileHeader fileHeader, CiphertextStreamsManager ciphertextStreamsManager, IFileSystemStatsTracker fileSystemStatsTracker)
        {
            this._security = security;
            this._fileHeader = fileHeader;
            this._ciphertextStreamsManager = ciphertextStreamsManager;
            this._fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public void WriteChunk(long chunkNumber, ICleartextChunk cleartextChunk)
        {
            AssertNotDisposed();
            
            if (cleartextChunk.NeedsFlush)
            {
                var ciphertextPosition = (chunkNumber * _security.ContentCryptor.FileContentCryptor.ChunkFullCiphertextSize) + _security.ContentCryptor.FileHeaderCryptor.HeaderSize;
                var ciphertextChunk = _security.ContentCryptor.FileContentCryptor.EncryptChunk(
                        cleartextChunk,
                        chunkNumber,
                        _fileHeader);

                _fileSystemStatsTracker?.AddBytesEncrypted(cleartextChunk.ActualLength);

                var ciphertextFileStream = _ciphertextStreamsManager.EnsureReadWriteStreamInstance();
                ciphertextFileStream.Position = ciphertextPosition;
                ciphertextFileStream.Write(ciphertextChunk.Buffer.Span);

                _fileSystemStatsTracker?.AddBytesWritten(ciphertextChunk.Buffer.Length);
            }
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
