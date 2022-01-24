using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Operations
{
    /// <summary>
    /// Provides module for managing directories on the file system.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IDirectoryOperations
    {
        DirectoryInfo CreateDirectory(string path);

        void DeleteDirectory(string path);

        void DeleteDirectory(string path, bool recursive);

        void Move(string sourcePath, string destinationPath);

        bool Exists(string path);

        IEnumerable<string> EnumerateFileSystemEntries(string path);

        IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string path);
    }
}
