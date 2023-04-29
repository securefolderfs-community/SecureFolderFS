using System;

namespace SecureFolderFS.Core.FileSystem.Directories
{
    /// <summary>
    /// Accesses directory IDs found on the encrypting file system.
    /// </summary>
    public interface IDirectoryIdAccess
    {
        /// <summary>
        /// Gets the directory ID of provided <paramref name="ciphertextPath"/> directory ID file path.
        /// </summary>
        /// <param name="ciphertextPath">The path to the ciphertext directory ID file.</param>
        /// <param name="directoryId">The <see cref="Span{T}"/> to fill the directory ID into.</param>
        /// <returns>If the <paramref name="directoryId"/> was retrieved successfully, return true, otherwise false.</returns>
        bool GetDirectoryId(string ciphertextPath, Span<byte> directoryId);

        /// <summary>
        /// Sets the directory ID of provided <paramref name="ciphertextPath"/> directory ID file path.
        /// </summary>
        /// <param name="ciphertextPath">The path to the ciphertext directory ID file.</param>
        /// <param name="directoryId">The ID to set for the directory.</param>
        /// <returns>Value is true if directory ID was successfully set, otherwise false.</returns>
        bool SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId);

        /// <summary>
        /// Removes associated directory ID from the list of known IDs.
        /// </summary>
        /// <param name="ciphertextPath">The path associated with a directory ID.</param>
        void RemoveDirectoryId(string ciphertextPath);
    }
}
