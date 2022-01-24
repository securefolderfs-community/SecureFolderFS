using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Operations
{
    /// <summary>
    /// Provides module for managing files on the file system.
    /// <br/>
    /// <br/>
    /// This API is exposed.
    /// </summary>
    public interface IFileOperations
    {
        Stream CreateFile(string path);

        Stream OpenFile(string path, FileMode mode);

        Stream OpenFile(string path, FileMode mode, FileAccess access);

        Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share);

        void DeleteFile(string path);

        void Move(string sourcePath, string destinationPath);

        bool Exists(string path);

        FileAttributes GetAttributes(string path);

        void SetAttributes(string path, FileAttributes attributes);

        void SetCreationTime(string path, DateTime creationTime);

        void SetLastAccessTime(string path, DateTime lastAccessTime);

        void SetLastWriteTime(string path, DateTime lastWriteTime);
    }
}
