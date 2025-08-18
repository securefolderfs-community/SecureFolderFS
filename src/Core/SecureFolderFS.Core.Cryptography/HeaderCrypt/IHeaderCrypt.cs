using System;

namespace SecureFolderFS.Core.Cryptography.HeaderCrypt
{
    /// <summary>
    /// Represents an encryption method to encrypt and decrypt headers.
    /// </summary>
    public interface IHeaderCrypt : IDisposable
    {
        /// <summary>
        /// Gets the size of ciphertext header.
        /// </summary>
        int HeaderCiphertextSize { get; }

        /// <summary>
        /// Gets the size of plaintext header.
        /// </summary>
        int HeaderPlaintextSize { get; }

        /// <summary>
        /// Creates a new plaintext header.
        /// </summary>
        /// <param name="plaintextHeader">The plaintext header to fill.</param>
        void CreateHeader(Span<byte> plaintextHeader);

        /// <summary>
        /// Encrypts the <paramref name="plaintextHeader"/> and writes <paramref name="ciphertextHeader"/> with encrypted data.
        /// </summary>
        /// <param name="plaintextHeader">The data to be encrypted with header nonce prepended.</param>
        /// <param name="ciphertextHeader">The encrypted data to be written to.</param>
        void EncryptHeader(ReadOnlySpan<byte> plaintextHeader, Span<byte> ciphertextHeader);

        /// <summary>
        /// Decrypts the <paramref name="ciphertextHeader"/> and writes <paramref name="plaintextHeader"/> with decrypted data.
        /// </summary>
        /// <param name="ciphertextHeader">The data to be decrypted.</param>
        /// <param name="plaintextHeader">The decrypted data to be written to with header nonce prepended.</param>
        bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> plaintextHeader);
    }
}
