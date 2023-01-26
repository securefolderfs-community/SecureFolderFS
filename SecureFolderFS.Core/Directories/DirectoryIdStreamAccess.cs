using SecureFolderFS.Core.FileSystem.Directories;
using System.IO;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdStreamAccess"/>
    public sealed class DirectoryIdStreamAccess : IDirectoryIdStreamAccess
    {
        /// <inheritdoc/>
        public Stream? OpenDirectoryIdStream(string ciphertextPath, FileMode mode, FileAccess access, FileShare share)
        {
            try
            {
                return File.Open(ciphertextPath, mode, access, share);
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
            catch (FileNotFoundException)
            {
                return null;
            }
        }
    }
}
