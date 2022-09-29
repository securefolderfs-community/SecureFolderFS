using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Buffers;
using System.Diagnostics;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkReader"/>
    internal sealed class ChunkReader : IChunkReader
    {
        private readonly ISecurity _security;
        private readonly BufferHolder _fileHeader;
        private readonly IStreamsManager _streamsManager;
        private readonly IFileSystemStatsTracker? _fileSystemStatsTracker;

        public ChunkReader(ISecurity security, BufferHolder fileHeader, IStreamsManager streamsManager, IFileSystemStatsTracker? fileSystemStatsTracker)
        {
            _security = security;
            _fileHeader = fileHeader;
            _streamsManager = streamsManager;
            _fileSystemStatsTracker = fileSystemStatsTracker;
        }

        /// <inheritdoc/>
        public int ReadChunk(long chunkNumber, Span<byte> cleartextChunk)
        {
            // Calculate sizes
            var ciphertextSize = _security.ContentCrypt.ChunkCiphertextSize;
            var ciphertextPosition = _security.HeaderCrypt.HeaderCiphertextSize + chunkNumber * ciphertextSize;

            // Rent array for ciphertext chunk
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);
            var realCiphertextChunk = ciphertextChunk.AsSpan(0, ciphertextSize);

            // Get and read from stream
            var ciphertextFileStream = _streamsManager.GetReadOnlyStream();
            _ = ciphertextFileStream ?? throw new UnavailableStreamException();
            ciphertextFileStream.Position = ciphertextPosition;
            var read = ciphertextFileStream.Read(realCiphertextChunk);

            _fileSystemStatsTracker?.AddBytesRead(read);

            // Check for end of file
            if (read == FileSystem.Constants.FILE_EOF)
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

            return read - (_security.ContentCrypt.ChunkCiphertextSize - _security.ContentCrypt.ChunkCleartextSize);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _streamsManager.Dispose();
        }
    }
}
