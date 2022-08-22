using SecureFolderFS.Core.FileHeaders;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using System;

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
            _security = security;
            _fileHeader = fileHeader;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            _fileSystemStatsTracker = fileSystemStatsTracker;
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

                var ciphertextFileStream = _ciphertextStreamsManager.GetReadWriteStreamInstance();
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
            _ciphertextStreamsManager?.Dispose();
        }
    }
}
