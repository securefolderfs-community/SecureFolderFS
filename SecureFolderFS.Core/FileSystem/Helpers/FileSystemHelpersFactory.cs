using System;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter.Dokan;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    internal sealed class FileSystemHelpersFactory
    {
        private readonly FileSystemAdapterType _fileSystemAdapterType;

        public FileSystemHelpersFactory(FileSystemAdapterType fileSystemAdapterType)
        {
            this._fileSystemAdapterType = fileSystemAdapterType;
        }

        public IFileSystemHelpers GetFileSystemHelpers()
        {
            return _fileSystemAdapterType switch
            {
                FileSystemAdapterType.DokanAdapter => new DokanFileSystemHelpers(),
                _ => throw new ArgumentOutOfRangeException(nameof(_fileSystemAdapterType))
            };
        }
    }
}
