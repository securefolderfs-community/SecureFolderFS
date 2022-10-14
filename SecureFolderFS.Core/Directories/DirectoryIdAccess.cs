using SecureFolderFS.Core.FileSystem.Directories;
using System;
using System.IO;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    internal sealed class DirectoryIdAccess : IDirectoryIdAccess
    {
        // TODO: Make this abstract and implement abstract caching - each file system adapter will provide file opening/creating impl
        // TODO: Make IDirectoryIdAccess support async and implement it in descendant callers as well

        /// <inheritdoc/>
        public DirectoryId? GetDirectoryId(string ciphertextPath)
        {
            try
            {
                var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);
                using var fileStream = File.Open(directoryIdPath, FileMode.Open, FileAccess.Read, FileShare.Read);

                var buffer = new byte[FileSystem.Constants.DIRECTORY_ID_SIZE];
                var read = fileStream.Read(buffer);
                if (read < buffer.Length)
                    return null;

                return new(buffer);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public bool SetDirectoryId(string ciphertextPath, DirectoryId directoryId)
        {
            try
            {
                var directoryIdPath = Path.Combine(ciphertextPath, FileSystem.Constants.DIRECTORY_ID_FILENAME);
                using var fileStream = File.Create(directoryIdPath);
                fileStream.Write(directoryId.Id);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public void RemoveDirectoryId(string ciphertextPath)
        {
            _ = ciphertextPath;
        }
    }
}
