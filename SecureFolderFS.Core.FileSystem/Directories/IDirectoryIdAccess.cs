using System;

namespace SecureFolderFS.Core.FileSystem.Directories
{
    /// <summary>
    /// Accesses DirectoryIDs found on the encrypting file system.
    /// </summary>
    public interface IDirectoryIdAccess
    {
        /// <summary>
        /// Gets the DirectoryID of provided DirectoryID <paramref name="ciphertextPath"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path to the DirectoryID file.</param>
        /// <param name="directoryId">The <see cref="Span{T}"/> to fill the DirectoryID into.</param>
        /// <returns>If the <paramref name="directoryId"/> was retrieved successfully; returns true, otherwise false.</returns>
        bool GetDirectoryId(string ciphertextPath, Span<byte> directoryId);

        /// <summary>
        /// Sets the DirectoryID of provided DirectoryID <paramref name="ciphertextPath"/> file path.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path to the DirectoryID file.</param>
        /// <param name="directoryId">The ID to set for the directory.</param>
        void SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId);

        /// <summary>
        /// Removes associated DirectoryID from the list of known IDs.
        /// </summary>
        /// <param name="ciphertextPath">The path associated with the DirectoryID.</param>
        void RemoveDirectoryId(string ciphertextPath);
    }
}
