using System;

namespace SecureFolderFS.Core.FileSystem.FileNames
{
    /// <summary>
    /// Accesses cleartext and ciphertext names of files and folders found on the encrypting file system.
    /// </summary>
    public interface IFileNameAccess
    {
        /// <summary>
        /// Gets cleartext name from associated <paramref name="ciphertextName"/>.
        /// </summary>
        /// <param name="ciphertextName">The associated ciphertext name.</param>
        /// <param name="directoryId">The ID of a directory where the item is stored.</param>
        /// <returns>If successful, returns a cleartext representation of the name; otherwise empty.</returns>
        string GetCleartextName(ReadOnlySpan<char> ciphertextName, ReadOnlySpan<byte> directoryId);

        /// <summary>
        /// Gets ciphertext name from associated <paramref name="cleartextName"/>.
        /// </summary>
        /// <param name="cleartextName">The associated cleartext name.</param>
        /// <param name="directoryId">The ID of a directory where the item is stored.</param>
        /// <returns>If successful, returns a ciphertext representation of the name; otherwise empty.</returns>
        string GetCiphertextName(ReadOnlySpan<char> cleartextName, ReadOnlySpan<byte> directoryId);
    }
}
