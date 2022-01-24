using System.Collections.Generic;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Operations.Implementation
{
    internal sealed class BuiltinDirectoryOperations : IDirectoryOperations
    {
        public DirectoryInfo CreateDirectory(string path)
        {
            return Directory.CreateDirectory(path);
        }

        public void DeleteDirectory(string path)
        {
            Directory.Delete(path);
        }

        public void DeleteDirectory(string path, bool recursive)
        {
            Directory.Delete(path, recursive);
        }

        public void Move(string sourcePath, string destinationPath)
        {
            Directory.Move(sourcePath, destinationPath);
        }

        public bool Exists(string path)
        {
            return Directory.Exists(path);
        }

        public IEnumerable<string> EnumerateFileSystemEntries(string path)
        {
            return Directory.EnumerateFileSystemEntries(path);
        }

        public IEnumerable<FileSystemInfo> EnumerateFileSystemInfos(string path)
        {
            return new DirectoryInfo(path).EnumerateFileSystemInfos();
        }
    }
}
