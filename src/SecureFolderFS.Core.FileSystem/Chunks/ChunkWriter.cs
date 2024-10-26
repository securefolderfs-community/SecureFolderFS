using SecureFolderFS.Core.Cryptography;
using SecureFolderFS.Core.FileSystem.Buffers;
using SecureFolderFS.Core.FileSystem.Exceptions;
using SecureFolderFS.Core.FileSystem.Streams;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Buffers;

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
        /// Writes <paramref name="cleartextChunk"/> into chunk at specified <paramref name="chunkNumber"/>.
        /// </summary>
        /// <param name="chunkNumber">The chunk number to write to.</param>
        /// <param name="cleartextChunk">The cleartext chunk to read from.</param>
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

            _fileSystemStatistics.BytesEncrypted?.Report(cleartextChunk.Length);

            // Get and write to ciphertext stream
            var ciphertextStream = _streamsManager.GetReadWriteStream();
            _ = ciphertextStream ?? throw new UnavailableStreamException();
            ciphertextStream.Position = streamPosition;
            ciphertextStream.Write(realCiphertextChunk);

            _fileSystemStatistics.BytesWritten?.Report(cleartextChunk.Length);

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
