using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides read access to chunks.
    /// </summary>
    internal sealed class ChunkReader
    {
        private readonly Security _security;
        private readonly BufferHolder _fileHeader;
        private readonly Stream _ciphertextStream;
        private readonly IFileSystemStatistics _fileSystemStatistics;

        public ChunkReader(Security security, BufferHolder fileHeader, Stream ciphertextStream, IFileSystemStatistics fileSystemStatistics)
        {
            _security = security;
            _fileHeader = fileHeader;
            _ciphertextStream = ciphertextStream;
            _fileSystemStatistics = fileSystemStatistics;
        }

        /// <summary>
        /// Reads chunk at specified <paramref name="chunkNumber"/> into <paramref name="plaintextChunk"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to read at.</param>
        /// <param name="plaintextChunk">The plaintext chunk to write to.</param>
        /// <returns>The number of plaintext bytes or -1 if integrity error occurred.</returns>
        public int ReadChunk(long chunkNumber, Span<byte> plaintextChunk)
        {
            // Calculate sizes
            var ciphertextSize = _security.ContentCrypt.ChunkCiphertextSize;
            var plaintextSize = _security.ContentCrypt.ChunkPlaintextSize;
            var ciphertextPosition = _security.HeaderCrypt.HeaderCiphertextSize + (chunkNumber * ciphertextSize);

            // Rent buffer
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);
            try
            {
                // ArrayPool may return a larger array than requested
                var realCiphertextChunk = ciphertextChunk.AsSpan(0, ciphertextSize);

                // Check position bounds
                if (_ciphertextStream.CanSeek && _ciphertextStream.Length < ciphertextPosition)
                    return 0;

                // Set the correct stream position
                if (!_ciphertextStream.TrySetPositionOrAdvance(ciphertextPosition))
                    return 0;

                // Return early if the stream is at the EOF position
                if (_ciphertextStream.IsEndOfStream())
                    return 0;

                // Read from the stream at the correct chunk
                var read = _ciphertextStream.Read(realCiphertextChunk);

                // Check for the end of the file
                if (read == Constants.FILE_EOF)
                    return 0;

                _fileSystemStatistics.BytesRead?.Report(read);

                // Get reserved part for ciphertext chunk
                var chunkReservedSize = Math.Min(read, _security.ContentCrypt.ChunkFirstReservedSize);
                var chunkReserved = realCiphertextChunk.Slice(0, chunkReservedSize);

                // Check if the reserved part is all zeros, in which case the decryption will be skipped (the chunk was extended)
                if (chunkReservedSize > 0 && SpanExtensions.IsAllZeros(chunkReserved))
                {
                    plaintextChunk.Clear();
                    return read - (ciphertextSize - plaintextSize);
                }

                // Decrypt
                var result = _security.ContentCrypt.DecryptChunk(
                    realCiphertextChunk.Slice(0, read),
                    chunkNumber,
                    _fileHeader,
                    plaintextChunk);

                _fileSystemStatistics.BytesDecrypted?.Report(read);

                // Check if the chunk is authentic
                if (!result)
                {
                    Debugger.Break();
                    return -1;
                }

                return read - (ciphertextSize - plaintextSize);
            }
            finally
            {
                // Clear ciphertext data before returning buffer to pool
                CryptographicOperations.ZeroMemory(ciphertextChunk.AsSpan(0, ciphertextSize));

                // Return buffer
                ArrayPool<byte>.Shared.Return(ciphertextChunk);
            }
        }
    }
}
