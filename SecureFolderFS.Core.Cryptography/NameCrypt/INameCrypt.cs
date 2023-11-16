using System;

namespace SecureFolderFS.Core.Cryptography.NameCrypt
{
    /// <summary>
    /// Represents an encryption method to encrypt and decrypt file/folder names.
    /// </summary>
    public interface INameCrypt : IDisposable
    {
        /// <summary>
        /// Encrypts the <paramref name="cleartextName"/> using associated <paramref name="directoryId"/>.
        /// </summary>
        /// <param name="cleartextName">The cleartext name to encrypt.</param>
        /// <param name="directoryId">The associated DirectoryID.</param>
        /// <returns>Encrypted ciphertext name.</returns>
        string EncryptName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId);

        /// <summary>
        /// Decrypts the <paramref name="ciphertextName"/> using associated <paramref name="directoryId"/>.
        /// </summary>
        /// <param name="ciphertextName">The ciphertext name to encrypt.</param>
        /// <param name="directoryId">The associated DirectoryID.</param>
        /// <returns>If the name was successfully decrypted, returns cleartext name, otherwise null.</returns>
        string? DecryptName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId);
    }
}
