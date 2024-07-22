using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    public static class DirectoryHelpers
    {
        public static byte[] AllocateDirectoryId(string path, FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return Array.Empty<byte>();

            if (path == Path.DirectorySeparatorChar.ToString())
                return Array.Empty<byte>();

            return new byte[Constants.DIRECTORY_ID_SIZE];
        }
    }
}
