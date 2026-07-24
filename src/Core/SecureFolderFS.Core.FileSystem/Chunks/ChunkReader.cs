using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
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

                // A legitimately sparse (SetLength-extended) or repaired chunk is zero-filled across its
                // ENTIRE length, so only a fully-zero chunk may skip authentication. Checking just the
                // reserved nonce would let an attacker with ciphertext write access zero those few bytes
                // to force any real chunk to decrypt as zeros with its MAC/AEAD tag never verified.
                // Requiring the whole chunk to be zero sends any partial tamper down the authenticated
                // path below, where the failed tag surfaces as an integrity error (-1).
                if (read > 0 && SpanExtensions.IsAllZeros(realCiphertextChunk.Slice(0, read)))
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
                    return -1;

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

        /// <inheritdoc cref="ReadChunk"/>
        public async ValueTask<int> ReadChunkAsync(long chunkNumber, Memory<byte> plaintextChunk, CancellationToken cancellationToken = default)
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
                var realCiphertextChunk = ciphertextChunk.AsMemory(0, ciphertextSize);

                // Check position bounds
                if (_ciphertextStream.CanSeek && _ciphertextStream.Length < ciphertextPosition)
                    return 0;

                // Set the correct stream position
                if (!await _ciphertextStream.TrySetPositionOrAdvanceAsync(ciphertextPosition, cancellationToken).ConfigureAwait(false))
                    return 0;

                // Return early if the stream is at the EOF position
                if (_ciphertextStream.IsEndOfStream())
                    return 0;

                // Read from the stream at the correct chunk
                var read = await _ciphertextStream.ReadAsync(realCiphertextChunk, cancellationToken).ConfigureAwait(false);

                // Check for the end of the file
                if (read == Constants.FILE_EOF)
                    return 0;

                _fileSystemStatistics.BytesRead?.Report(read);

                // A legitimately sparse (SetLength-extended) or repaired chunk is zero-filled across its
                // ENTIRE length, so only a fully-zero chunk may skip authentication. Checking just the
                // reserved nonce would let an attacker with ciphertext write access zero those few bytes
                // to force any real chunk to decrypt as zeros with its MAC/AEAD tag never verified.
                // Requiring the whole chunk to be zero sends any partial tamper down the authenticated
                // path below, where the failed tag surfaces as an integrity error (-1).
                if (read > 0 && SpanExtensions.IsAllZeros(realCiphertextChunk.Span.Slice(0, read)))
                {
                    plaintextChunk.Span.Clear();
                    return read - (ciphertextSize - plaintextSize);
                }

                // Decrypt
                var result = _security.ContentCrypt.DecryptChunk(
                    realCiphertextChunk.Span.Slice(0, read),
                    chunkNumber,
                    _fileHeader,
                    plaintextChunk.Span);

                _fileSystemStatistics.BytesDecrypted?.Report(read);

                // Check if the chunk is authentic
                if (!result)
                    return -1;

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
