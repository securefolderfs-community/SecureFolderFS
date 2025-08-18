using System;
using static SecureFolderFS.Core.Cryptography.Constants.Crypto.Chunks.Empty;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <inheritdoc cref="IContentCrypt"/>
    internal sealed class NoContentCrypt : BaseContentCrypt
    {
        /// <inheritdoc/>
        public override int ChunkPlaintextSize { get; } = CHUNK_PLAINTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkCiphertextSize { get; } = CHUNK_CIPHERTEXT_SIZE;

        /// <inheritdoc/>
        public override int ChunkFirstReservedSize { get; } = 0;

        /// <inheritdoc/>
        public override void EncryptChunk(ReadOnlySpan<byte> plaintextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk)
        {
            plaintextChunk.CopyTo(ciphertextChunk);
        }

        /// <inheritdoc/>
        public override bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> plaintextChunk)
        {
            ciphertextChunk.CopyTo(plaintextChunk);
            return true;
        }

        /// <inheritdoc/>
        public override long CalculateCiphertextSize(long plaintextSize)
        {
            return plaintextSize;
        }

        /// <inheritdoc/>
        public override long CalculatePlaintextSize(long ciphertextSize)
        {
            return ciphertextSize;
        }
    }
}