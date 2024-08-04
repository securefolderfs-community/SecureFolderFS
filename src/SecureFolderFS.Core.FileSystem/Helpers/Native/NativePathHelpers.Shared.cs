using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers.Native
{
    /// <summary>
    /// A set of file system path management helpers that only work in a native environment with unlimited file system access.
    /// </summary>
    public static partial class NativePathHelpers
    {
        public static string? GetDirectoryIdPathOfParent(string ciphertextChildPath, string rootPath)
        {
            if (ciphertextChildPath == Path.DirectorySeparatorChar.ToString() || ciphertextChildPath.Equals(rootPath))
                return string.Empty;

            var parentPath = Path.GetDirectoryName(ciphertextChildPath);
            if (parentPath is null)
                return null;

            // Parent path is the same as rootPath where the DirectoryID should be empty
            if (parentPath == Path.DirectorySeparatorChar.ToString() || parentPath.Equals(rootPath))
                return string.Empty;

            return Path.Combine(parentPath, Constants.DIRECTORY_ID_FILENAME);
        }

        [Obsolete]
        public static string PathFromVaultRoot(string fileName, string vaultRootPath)
        {
            if (fileName.Length == 0 || fileName.Length == 1 && fileName[0] == Path.DirectorySeparatorChar)
                return PathHelpers.EnsureTrailingPathSeparator(vaultRootPath);

            return Path.Combine(vaultRootPath, PathHelpers.EnsureNoLeadingPathSeparator(fileName));
        }

        public static string MakeRelative(string fullPath, string basePath)
        {
            return PathHelpers.EnsureNoLeadingPathSeparator(Path.DirectorySeparatorChar + fullPath.Replace(basePath, ""));
        }

        public static bool GetDirectoryId(string ciphertextChildPath, FileSystemSpecifics specifics, Span<byte> directoryId)
        {
            var directoryIdPath = GetDirectoryIdPathOfParent(ciphertextChildPath, specifics.ContentFolder.Id);
            if (directoryIdPath is null)
                throw new InvalidOperationException("Could not get Directory ID path.");

            // If the Directory ID path is root, return false
            if (directoryIdPath == string.Empty)
                return false;

            // Check directoryId size once we actually get to the reading
            if (directoryId.Length < Constants.DIRECTORY_ID_SIZE)
                throw new ArgumentException($"The {nameof(directoryId)} is of incorrect size: {directoryId.Length}.");

            var cachedId = specifics.DirectoryIdCache.CacheGet(directoryIdPath);
            if (cachedId is not null)
            {
                cachedId.Buffer.CopyTo(directoryId);
                return true;
            }

            int read;
            using var directoryIdStream = File.Open(directoryIdPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedId = new(Constants.DIRECTORY_ID_SIZE);
                read = directoryIdStream.Read(cachedId);
                specifics.DirectoryIdCache.CacheSet(directoryIdPath, cachedId);
            }
            else
                read = directoryIdStream.Read(directoryId);

            if (read < Constants.DIRECTORY_ID_SIZE)
                throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }
    }
}
