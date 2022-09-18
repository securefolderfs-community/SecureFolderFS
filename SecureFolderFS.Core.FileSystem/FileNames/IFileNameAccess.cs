using SecureFolderFS.Core.FileSystem.Directories;

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
        /// <param name="directoryId">The ID of directory where the file/folder is stored.</param>
        /// <returns>A cleartext representation of the name.</returns>
        string GetCleartextName(string ciphertextName, DirectoryId directoryId);

        /// <summary>
        /// Gets ciphertext name from associated <paramref name="cleartextFileName"/>.
        /// </summary>
        /// <param name="cleartextFileName">The associated cleartext name.</param>
        /// <param name="directoryId">The ID of directory where the file/folder is stored.</param>
        /// <returns>A ciphertext representation of the name.</returns>
        string GetCiphertextName(string cleartextFileName, DirectoryId directoryId);
    }
}
