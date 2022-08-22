using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using System;
using System.Buffers;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkWriter"/>
    internal sealed class ChunkWriter : IChunkWriter
    {
        private readonly ISecurity _security;
        private readonly ReferenceBuffer _fileHeader;
        private readonly CiphertextStreamsManager _ciphertextStreamsManager;
        private readonly IFileSystemStatsTracker? _fileSystemStatsTracker;

        public ChunkWriter(ISecurity security, ReferenceBuffer fileHeader, CiphertextStreamsManager ciphertextStreamsManager, IFileSystemStatsTracker? fileSystemStatsTracker)
        {
            _security = security;
            _fileHeader = fileHeader;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            _fileSystemStatsTracker = fileSystemStatsTracker;
        }

        /// <inheritdoc/>
        public void WriteChunk(long chunkNumber, ReadOnlySpan<byte> cleartextChunk)
        {
            // Calculate size of ciphertext
            var ciphertextSize = Math.Min(cleartextChunk.Length + _security.ContentCrypt.ChunkCiphertextOverheadSize, _security.ContentCrypt.ChunkCiphertextSize);

            // Calculate position in ciphertext stream
            var streamPosition = _security.HeaderCrypt.HeaderCiphertextSize + (chunkNumber * _security.ContentCrypt.ChunkCiphertextSize);

            // Rent array for ciphertext chunk
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);

            // Encrypt
            _security.ContentCrypt.EncryptChunk(
                cleartextChunk,
                chunkNumber,
                _fileHeader,
                ciphertextChunk);

            _fileSystemStatsTracker?.AddBytesEncrypted(cleartextChunk.Length);

            // Get and write to ciphertext stream
            var ciphertextStream = _ciphertextStreamsManager.GetReadWriteStreamInstance();
            ciphertextStream.Position = streamPosition;
            ciphertextStream.Write(ciphertextChunk);

            _fileSystemStatsTracker?.AddBytesWritten(cleartextChunk.Length);

            // Return array
            ArrayPool<byte>.Shared.Return(ciphertextChunk);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _ciphertextStreamsManager.Dispose();
        }
    }
}
