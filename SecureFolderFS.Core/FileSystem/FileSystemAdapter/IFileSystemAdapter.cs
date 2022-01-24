using System;

namespace SecureFolderFS.Core.FileSystem.FileSystemAdapter
{
    internal interface IFileSystemAdapter : IDisposable
    {
        void StartFileSystem(string mountLocation);

        bool StopFileSystem(string mountLocation);
    }
}
