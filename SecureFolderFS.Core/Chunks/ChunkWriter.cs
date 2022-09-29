using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using System;
using System.Buffers;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkWriter"/>
    internal sealed class ChunkWriter : IChunkWriter
    {
        private readonly ISecurity _security;
        private readonly HeaderBuffer _fileHeader;
        private readonly IStreamsManager _streamsManager;
        private readonly IFileSystemStatsTracker? _fileSystemStatsTracker;

        public ChunkWriter(ISecurity security, HeaderBuffer fileHeader, IStreamsManager streamsManager, IFileSystemStatsTracker? fileSystemStatsTracker)
        {
            _security = security;
            _fileHeader = fileHeader;
            _streamsManager = streamsManager;
            _fileSystemStatsTracker = fileSystemStatsTracker;
        }

        /// <inheritdoc/>
        public void WriteChunk(long chunkNumber, ReadOnlySpan<byte> cleartextChunk)
        {
            // Calculate size of ciphertext
            var ciphertextSize = Math.Min(cleartextChunk.Length + (_security.ContentCrypt.ChunkCiphertextSize - _security.ContentCrypt.ChunkCleartextSize), _security.ContentCrypt.ChunkCiphertextSize);

            // Calculate position in ciphertext stream
            var streamPosition = _security.HeaderCrypt.HeaderCiphertextSize + chunkNumber * _security.ContentCrypt.ChunkCiphertextSize;

            // Rent array for ciphertext chunk
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);
            var realCiphertextChunk = ciphertextChunk.AsSpan(0, ciphertextSize);

            // Encrypt
            _security.ContentCrypt.EncryptChunk(
                cleartextChunk,
                chunkNumber,
                _fileHeader,
                realCiphertextChunk);

            _fileSystemStatsTracker?.AddBytesEncrypted(cleartextChunk.Length);

            // Get and write to ciphertext stream
            var ciphertextStream = _streamsManager.GetReadWriteStream();
            _ = ciphertextStream ?? throw new UnavailableStreamException();
            ciphertextStream.Position = streamPosition;
            ciphertextStream.Write(realCiphertextChunk);

            _fileSystemStatsTracker?.AddBytesWritten(cleartextChunk.Length);

            // Return array
            ArrayPool<byte>.Shared.Return(ciphertextChunk);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _streamsManager.Dispose();
        }
    }
}
