using System;

namespace SecureFolderFS.Core.Cryptography.ContentCrypt
{
    /// <summary>
    /// Represents an encryption method to encrypt and decrypt content chunks.
    /// </summary>
    public interface IContentCrypt : IDisposable
    {
        /// <summary>
        /// Gets the size of plaintext chunk.
        /// </summary>
        int ChunkPlaintextSize { get; }

        /// <summary>
        /// Gets the full size of ciphertext chunk.
        /// </summary>
        int ChunkCiphertextSize { get; }

        /// <summary>
        /// Gets the first reserved overhead size of ciphertext chunk.
        /// </summary>
        int ChunkFirstReservedSize { get; }

        /// <summary>
        /// Encrypts the <paramref name="plaintextChunk"/> and writes <paramref name="ciphertextChunk"/> with encrypted data.
        /// </summary>
        /// <param name="plaintextChunk">The data to be encrypted.</param>
        /// <param name="chunkNumber">The chunk number part of encrypted data.</param>
        /// <param name="header">The header of the whole data.</param>
        /// <param name="ciphertextChunk">The encrypted data to be written to.</param>
        void EncryptChunk(ReadOnlySpan<byte> plaintextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> ciphertextChunk);

        /// <summary>
        /// Decrypts the <paramref name="ciphertextChunk"/> and writes <paramref name="plaintextChunk"/> with decrypted data.
        /// </summary>
        /// <param name="ciphertextChunk">The data to be decrypted.</param>
        /// <param name="chunkNumber">The chunk number part of encrypted data.</param>
        /// <param name="header">The header of the whole data.</param>
        /// <param name="plaintextChunk">The decrypted data to be written to.</param>
        /// <returns>True if decryption was successful; otherwise false if an integrity error occurred.</returns>
        bool DecryptChunk(ReadOnlySpan<byte> ciphertextChunk, long chunkNumber, ReadOnlySpan<byte> header, Span<byte> plaintextChunk);

        /// <summary>
        /// Calculates the ciphertext size from <paramref name="plaintextSize"/>.
        /// </summary>
        /// <param name="plaintextSize">The size of plaintext.</param>
        /// <returns>Aligned ciphertext size.</returns>
        long CalculateCiphertextSize(long plaintextSize);

        /// <summary>
        /// Calculates the plaintext size from <paramref name="ciphertextSize"/>.
        /// </summary>
        /// <param name="ciphertextSize">The size of ciphertext.</param>
        /// <returns>Aligned ciphertext size. Value can be -1 if the provided <paramref name="ciphertextSize"/> is incorrect.</returns>
        long CalculatePlaintextSize(long ciphertextSize);
    }
}
