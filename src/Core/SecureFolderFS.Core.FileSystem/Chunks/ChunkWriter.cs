using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Buffers;
using System.Security.Cryptography;

namespace SecureFolderFS.Core.FileSystem.Chunks
{
    /// <summary>
    /// Provides write access to chunks.
    /// </summary>
    internal sealed class ChunkWriter : IDisposable
    {
        private readonly Security _security;
        private readonly HeaderBuffer _fileHeader;
        private readonly StreamsManager _streamsManager;
        private readonly IFileSystemStatistics _fileSystemStatistics;

        public ChunkWriter(Security security, HeaderBuffer fileHeader, StreamsManager streamsManager, IFileSystemStatistics fileSystemStatistics)
        {
            _security = security;
            _fileHeader = fileHeader;
            _streamsManager = streamsManager;
            _fileSystemStatistics = fileSystemStatistics;
        }

        /// <summary>
        /// Writes <paramref name="plaintextChunk"/> into chunk at specified <paramref name="chunkNumber"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to write to.</param>
        /// <param name="plaintextChunk">The plaintext chunk to read from.</param>
        public void WriteChunk(long chunkNumber, ReadOnlySpan<byte> plaintextChunk)
        {
            // Calculate size of ciphertext
            var ciphertextSize = Math.Min(plaintextChunk.Length + (_security.ContentCrypt.ChunkCiphertextSize - _security.ContentCrypt.ChunkPlaintextSize), _security.ContentCrypt.ChunkCiphertextSize);

            // Calculate position in ciphertext stream
            var streamPosition = _security.HeaderCrypt.HeaderCiphertextSize + chunkNumber * _security.ContentCrypt.ChunkCiphertextSize;

            // Rent buffer
            var ciphertextChunk = ArrayPool<byte>.Shared.Rent(ciphertextSize);
            try
            {
                // ArrayPool may return a larger array than requested
                var realCiphertextChunk = ciphertextChunk.AsSpan(0, ciphertextSize);

                // Encrypt
                _security.ContentCrypt.EncryptChunk(
                    plaintextChunk,
                    chunkNumber,
                    _fileHeader,
                    realCiphertextChunk);

                _fileSystemStatistics.BytesEncrypted?.Report(plaintextChunk.Length);

                // Get available read-write stream or throw
                var ciphertextStream = _streamsManager.GetReadWriteStream();
                _ = ciphertextStream ?? throw new UnavailableStreamException();

                // Check position bounds
                if (streamPosition > ciphertextStream.Length)
                    return;

                // Set the correct stream position
                if (!ciphertextStream.TrySetPositionOrAdvance(streamPosition))
                    return;

                // Write to stream at the correct chunk
                ciphertextStream.Write(realCiphertextChunk);

                _fileSystemStatistics.BytesWritten?.Report(realCiphertextChunk.Length);
            }
            finally
            {
                // Clear ciphertext data before returning buffer to pool
                CryptographicOperations.ZeroMemory(ciphertextChunk.AsSpan(0, ciphertextSize));

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
