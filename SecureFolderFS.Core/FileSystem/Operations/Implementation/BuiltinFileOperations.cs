using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Operations.Implementation
{
    internal sealed class BuiltinFileOperations : IFileOperations
    {
        public Stream CreateFile(string path)
        {
            return File.Create(path);
        }

        public Stream OpenFile(string path, FileMode mode)
        {
            return File.Open(path, mode);
        }

        public Stream OpenFile(string path, FileMode mode, FileAccess access)
        {
            return File.Open(path, mode, access);
        }

        public Stream OpenFile(string path, FileMode mode, FileAccess access, FileShare share)
        {
            return File.Open(path, mode, access, share);
        }

        public void DeleteFile(string path)
        {
            File.Delete(path);
        }

        public void Move(string sourcePath, string destinationPath)
        {
            File.Move(sourcePath, destinationPath);
        }

        public bool Exists(string path)
        {
            return File.Exists(path);
        }

        public FileAttributes GetAttributes(string path)
        {
            return File.GetAttributes(path);
        }

        public void SetAttributes(string path, FileAttributes attributes)
        {
            File.SetAttributes(path, attributes);
        }

        public void SetCreationTime(string path, DateTime creationTime)
        {
            File.SetCreationTime(path, creationTime);
        }

        public void SetLastAccessTime(string path, DateTime lastAccessTime)
        {
            File.SetLastAccessTime(path, lastAccessTime);
        }

        public void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            File.SetLastWriteTime(path, lastWriteTime);
        }
    }
}
