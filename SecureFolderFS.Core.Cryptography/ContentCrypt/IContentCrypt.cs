using System;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <summary>
    /// Represents an encryption method to encrypt and decrypt content chunks.
    /// </summary>
    public interface IContentCrypt : IDisposable
    {
        /// <summary>
        /// Gets the size of cleartext chunk.
        /// </summary>
        int ChunkCleartextSize { get; }

        /// <summary>
        /// Gets the full size of ciphertext chunk.
        /// </summary>
        int ChunkCiphertextSize { get; }

        /// <summary>
        /// Gets the size of overhead per ciphertext chunk.
        /// </summary>
        int ChunkCiphertextOverheadSize { get; }

        /// <summary>
        /// Encrypts the <paramref name="cleartextChunk"/> and writes <paramref name="ciphertextChunk"/> with encrypted data.
        /// </summary>
        /// <param name="cleartextChunk">The data to be encrypted.</param>
        /// <param name="chunkNumber">The chunk number part of encrypted data.</param>
        /// <param name="header">The header of the whole data.</param>
        /// <param name="ciphertextChunk">The encrypted data to be written to.</param>
        void EncryptChunk(ReadOnlySpan<byte> cleartextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk);

        /// <summary>
        /// Decrypts the <paramref name="ciphertextChunk"/> and writes <paramref name="cleartextChunk"/> with decrypted data.
        /// </summary>
        /// <param name="ciphertextChunk">The data to be decrypted.</param>
        /// <param name="chunkNumber">The chunk number part of encrypted data.</param>
        /// <param name="header">The header of the whole data.</param>
        /// <param name="cleartextChunk">The decrypted data to be written to.</param>
        /// <returns>True if decryption was successful, otherwise false if an integrity error occurred.</returns>
        bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> cleartextChunk);

        /// <summary>
        /// Calculates the ciphertext size from <paramref name="cleartextSize"/>.
        /// </summary>
        /// <param name="cleartextSize">The size of cleartext.</param>
        /// <returns>Aligned ciphertext size.</returns>
        long CalculateCiphertextSize(long cleartextSize);

        /// <summary>
        /// Calculates the cleartext size from <paramref name="ciphertextSize"/>.
        /// </summary>
        /// <param name="ciphertextSize">The size of ciphertext.</param>
        /// <returns>Aligned ciphertext size. Value can be -1 if the provided <paramref name="ciphertextSize"/> is incorrect.</returns>
        long CalculateCleartextSize(long ciphertextSize);
    }
}
