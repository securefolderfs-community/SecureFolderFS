using DokanNet;
using SecureFolderFS.Core.Enums;
using System;

namespace SecureFolderFS.Core.Helpers
{
    internal static class FileSystemHelpers
    {
        public static FileSystemFeatures ToDokanFileSystemFlags(this FileSystemFlags fileSystemFlags)
        {
            return (FileSystemFeatures)(uint)fileSystemFlags;
        }
    }
}
