using SecureFolderFS.Core.BufferHolders;
using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Streams.Management;
using System;
using System.Buffers;
using System.Diagnostics;

namespace SecureFolderFS.Core.Chunks.ChunkAccessImpl
{
    /// <inheritdoc cref="IChunkReader"/>
    internal sealed class ChunkReader : IChunkReader
    {
        private readonly ISecurity _security;
        private readonly CleartextHeaderBuffer _fileHeader;
        private readonly CiphertextStreamsManager _ciphertextStreamsManager;
        private readonly IFileSystemStatsTracker? _fileSystemStatsTracker;

        public ChunkReader(ISecurity security, CleartextHeaderBuffer fileHeader, CiphertextStreamsManager ciphertextStreamsManager, IFileSystemStatsTracker? fileSystemStatsTracker)
        {
            _security = security;
            _fileHeader = fileHeader;
            _ciphertextStreamsManager = ciphertextStreamsManager;
            _fileSystemStatsTracker = fileSystemStatsTracker;
        }

        /// <inheritdoc/>
        public int ReadChunk(long chunkNumber, Span<byte> cleartextChunk)
        {
            // Calculate sizes
            var ciphertextSize = _security.ContentCrypt.ChunkCiphertextSize;
            var ciphertextPosition = _security.HeaderCrypt.HeaderCiphertextSize + (chunkNumber * ciphertextSize);

            // Rent array for ciphertext chunk
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);
            var realCiphertextChunk = ciphertextChunk.AsSpan(0, ciphertextSize);

            // Get and read from stream
            var ciphertextFileStream = _ciphertextStreamsManager.GetReadOnlyStreamInstance();
            ciphertextFileStream.Position = ciphertextPosition;
            var read = ciphertextFileStream.Read(realCiphertextChunk);

            _fileSystemStatsTracker?.AddBytesRead(read);

            // Check for end of file
            if (read == Constants.IO.FILE_EOF)
            {
                ArrayPool<byte>.Shared.Return(ciphertextChunk);
                return 0;
            }

            // Decrypt
            var result = _security.ContentCrypt.DecryptChunk(
                realCiphertextChunk.Slice(0, read),
                chunkNumber,
                _fileHeader,
                cleartextChunk);

            _fileSystemStatsTracker?.AddBytesDecrypted(read);

            // Return array
            ArrayPool<byte>.Shared.Return(ciphertextChunk);

            if (!result)
            {
                Debugger.Break();
                return -1;
            }

            return read - _security.ContentCrypt.ChunkCiphertextOverheadSize;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _ciphertextStreamsManager.Dispose();
        }
    }
}
