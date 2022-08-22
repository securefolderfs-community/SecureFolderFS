using System;

namespace SecureFolderFS.Core.Security.ContentCrypt.FileHeader
{
    internal interface IHeaderCrypt : IDisposable
    {
        /// <summary>
        /// Gets the size of ciphertext header.
        /// </summary>
        int HeaderCiphertextSize { get; }

        /// <summary>
        /// Creates a new cleartext header.
        /// </summary>
        /// <param name="cleartextHeader">The cleartext header to fill.</param>
        void CreateHeader(Span<byte> cleartextHeader);

        /// <summary>
        /// Encrypts the <paramref name="cleartextHeader"/> and writes <paramref name="ciphertextHeader"/> with encrypted data.
        /// </summary>
        /// <param name="cleartextHeader">The data to be encrypted with header nonce prepended.</param>
        /// <param name="ciphertextHeader">The encrypted data to be written to.</param>
        void EncryptHeader(ReadOnlySpan<byte> cleartextHeader, Span<byte> ciphertextHeader);

        /// <summary>
        /// Decrypts the <paramref name="ciphertextHeader"/> and writes <paramref name="cleartextHeader"/> with decrypted data.
        /// </summary>
        /// <param name="ciphertextHeader">The data to be decrypted.</param>
        /// <param name="cleartextHeader">The decrypted data to be written to with header nonce prepended.</param>
        bool DecryptHeader(ReadOnlySpan<byte> ciphertextHeader, Span<byte> cleartextHeader);
    }
}
