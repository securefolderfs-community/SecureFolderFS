using System.IO;

namespace SecureFolderFS.Core.FileSystem.Directories
{
    /// <summary>
    /// Accesses files containing directory IDs found on the encrypting file system.
    /// </summary>
    public interface IDirectoryIdStreamAccess
    {
        /// <summary>
        /// Opens a stream to directory ID data file.
        /// </summary>
        /// <param name="ciphertextPath">The path pointing to directory ID on the ciphertext file system.</param>
        /// <param name="mode">The <see cref="FileMode"/> to open the file with.</param>
        /// <param name="access">The <see cref="FileAccess"/> to open the file with.</param>
        /// <param name="share">The <see cref="FileShare"/> to open the file with.</param>
        /// <returns>If successful, returns a <see cref="Stream"/> instance containing the directory ID, otherwise null.</returns>
        Stream? OpenDirectoryIdStream(string ciphertextPath, FileMode mode, FileAccess access, FileShare share);
    }
}
