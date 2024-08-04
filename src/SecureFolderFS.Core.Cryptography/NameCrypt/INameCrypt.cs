using System;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <summary>
    /// Represents an encryption method to encrypt and decrypt file and folder names.
    /// </summary>
    public interface INameCrypt : IDisposable
    {
        /// <summary>
        /// Encrypts the <paramref name="plaintextName"/> using associated <paramref name="directoryId"/>.
        /// </summary>
        /// <param name="plaintextName">The plaintext name to encrypt.</param>
        /// <param name="directoryId">The Directory ID that the file name is a part of. Can accept <see cref="ReadOnlySpan{T}.Empty"/>.</param>
        /// <returns>Encrypted ciphertext name.</returns>
        string EncryptName(ReadOnlySpan<char> plaintextName, ReadOnlySpan<byte> directoryId);

        /// <summary>
        /// Decrypts the <paramref name="ciphertextName"/> using associated <paramref name="directoryId"/>.
        /// </summary>
        /// <param name="ciphertextName">The ciphertext name to encrypt.</param>
        /// <param name="directoryId">The Directory ID that the file name is a part of. Can accept <see cref="ReadOnlySpan{T}.Empty"/>.</param>
        /// <returns>If the name was successfully decrypted, returns plaintext name; otherwise null.</returns>
        string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId);
    }
}
