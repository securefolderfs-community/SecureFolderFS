using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers.Abstract
{
    /// <summary>
    /// A set of file system path management helpers that work on any platform including constrained environments with limited file system access.
    /// </summary>
    public static partial class AbstractPathHelpers
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
