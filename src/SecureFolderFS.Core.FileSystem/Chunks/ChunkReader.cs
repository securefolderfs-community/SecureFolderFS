using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System;
using System.Buffers;
using System.Diagnostics;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides read access to chunks.
    /// </summary>
    internal sealed class ChunkReader : IDisposable
    {
        private readonly Security _security;
        private readonly BufferHolder _fileHeader;
        private readonly StreamsManager _streamsManager;
        private readonly IFileSystemStatistics _fileSystemStatistics;

        public ChunkReader(Security security, BufferHolder fileHeader, StreamsManager streamsManager, IFileSystemStatistics fileSystemStatistics)
        {
            _security = security;
            _fileHeader = fileHeader;
            _streamsManager = streamsManager;
            _fileSystemStatistics = fileSystemStatistics;
        }

        /// <summary>
        /// Reads chunk at specified <paramref name="chunkNumber"/> into <paramref name="cleartextChunk"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to read at.</param>
        /// <param name="cleartextChunk">The cleartext chunk to write to.</param>
        /// <returns>The amount of cleartext bytes or -1 if integrity error occurred.</returns>
        public int ReadChunk(long chunkNumber, Span<byte> cleartextChunk)
        {
            // Calculate sizes
            var ciphertextSize = _security.ContentCrypt.ChunkCiphertextSize;
            var cleartextSize = _security.ContentCrypt.ChunkCleartextSize;
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

                _fileSystemStatistics.BytesRead?.Report(read);

                // Get reserved part for ciphertext chunk
                var chunkReservedSize = Math.Min(read, _security.ContentCrypt.ChunkFirstReservedSize);
                var chunkReserved = realCiphertextChunk.Slice(0, chunkReservedSize);

                // Check if the reserved part is all zeros in which case the decryption will be skipped (the chunk was extended)
                if (SpanExtensions.IsAllZeros(chunkReserved))
                {
                    cleartextChunk.Clear();
                    return read - (ciphertextSize - cleartextSize);
                }

                // Decrypt
                var result = _security.ContentCrypt.DecryptChunk(
                    realCiphertextChunk.Slice(0, read),
                    chunkNumber,
                    _fileHeader,
                    cleartextChunk);

                _fileSystemStatistics.BytesDecrypted?.Report(read);

                // Check if the chunk is authentic
                if (!result)
                {
                    Debugger.Break();
                    return -1;
                }

                return read - (ciphertextSize - cleartextSize);
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
