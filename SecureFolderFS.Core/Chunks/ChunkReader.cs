using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Chunks;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Buffers;
using System.Diagnostics;

namespace SecureFolderFS.Core.Chunks
{
    /// <inheritdoc cref="IChunkReader"/>
    internal sealed class ChunkReader : IChunkReader
    {
        private readonly Security _security;
        private readonly BufferHolder _fileHeader;
        private readonly IStreamsManager _streamsManager;
        private readonly IFileSystemStatistics? _fileSystemStatistics;

        public ChunkReader(Security security, BufferHolder fileHeader, IStreamsManager streamsManager, IFileSystemStatistics? fileSystemStatistics)
        {
            _security = security;
            _fileHeader = fileHeader;
            _streamsManager = streamsManager;
            _fileSystemStatistics = fileSystemStatistics;
        }

        /// <inheritdoc/>
        public int ReadChunk(long chunkNumber, Span<byte> cleartextChunk)
        {
            // Calculate sizes
            var ciphertextSize = _security.ContentCrypt.ChunkCiphertextSize;
            var ciphertextPosition = _security.HeaderCrypt.HeaderCiphertextSize + (chunkNumber * ciphertextSize);

            // Rent buffer
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);
            try
            {
                // ArrayPool may return larger array than requested
                var realCiphertextChunk = ciphertextChunk.AsSpan(0, ciphertextSize);

                // Get available read stream or throw
                var ciphertextFileStream = _streamsManager.GetReadOnlyStream();
                _ = ciphertextFileStream ?? throw new UnavailableStreamException();

                // Read from stream at correct chunk
                ciphertextFileStream.Position = ciphertextPosition;
                var read = ciphertextFileStream.Read(realCiphertextChunk);

                // Check for end of file
                if (read == FileSystem.Constants.FILE_EOF)
                    return 0;

                _fileSystemStatistics?.NotifyBytesRead(read);

                // Get reserved part for ciphertext chunk
                var chunkReservedSize = Math.Min(read, _security.ContentCrypt.ChunkFirstReservedSize);
                var chunkReserved = realCiphertextChunk.Slice(0, chunkReservedSize);

                // Check if the reserved part is all zeros in which case the decryption will be skipped (the chunk was extended)
                if (SpanExtensions.IsAllZeros(chunkReserved))
                {
                    cleartextChunk.Clear();
                    return read;
                }

                // Decrypt
                var result = _security.ContentCrypt.DecryptChunk(
                    realCiphertextChunk.Slice(0, read),
                    chunkNumber,
                    _fileHeader,
                    cleartextChunk);

                _fileSystemStatistics?.NotifyBytesDecrypted(read);

                // Check if the chunk is authentic
                if (!result)
                {
                    Debugger.Break();
                    return -1;
                }

                return read - (_security.ContentCrypt.ChunkCiphertextSize - _security.ContentCrypt.ChunkCleartextSize);
            }
            finally
            {
                // Return buffer
                ArrayPool<byte>.Shared.Return(ciphertextChunk);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _streamsManager.Dispose();
        }
    }
}
