using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
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

            return Path.Combine(parentPath, Constants.Names.DIRECTORY_ID_FILENAME);
        }

        public static bool GetDirectoryId(string ciphertextFolderPath, FileSystemSpecifics specifics, Span<byte> directoryId)
        {
            // Check if we're at the root
            if (ciphertextFolderPath == Path.DirectorySeparatorChar.ToString() || ciphertextFolderPath.Equals(specifics.ContentFolder.Id))
                return false;

            // Check directoryId size once we actually get to the reading
            if (directoryId.Length < Constants.DIRECTORY_ID_SIZE)
                throw new ArgumentException($"The {nameof(directoryId)} is of incorrect size: {directoryId.Length}.");

            // Try to get the Directory ID from the cache first
            var directoryIdPath = Path.Combine(ciphertextFolderPath, Constants.Names.DIRECTORY_ID_FILENAME);
            var cachedDirectoryId = specifics.DirectoryIdCache.CacheGet(directoryIdPath);
            if (cachedDirectoryId is not null)
            {
                cachedDirectoryId.Buffer.CopyTo(directoryId);
                return true;
            }

            // Read the Directory ID from the file
            using var directoryIdStream = File.Open(directoryIdPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (specifics.DirectoryIdCache.IsAvailable)
            {
                cachedDirectoryId = new(Constants.DIRECTORY_ID_SIZE);
                var read = directoryIdStream.Read(cachedDirectoryId);
                if (read < Constants.DIRECTORY_ID_SIZE)
                    throw new IOException($"The data inside the Directory ID file is of incorrect size: {read}.");

                cachedDirectoryId.Buffer.CopyTo(directoryId);
                specifics.DirectoryIdCache.CacheSet(directoryIdPath, cachedDirectoryId);
            }
            else
            {
                var read = directoryIdStream.Read(directoryId);
                if (read < Constants.DIRECTORY_ID_SIZE)
                    throw new IOException($"The data inside the Directory ID file is of incorrect size: {read}.");
            }

            // The Directory ID is not empty - return true
            return true;
        }

        [Obsolete($"Use {nameof(GetDirectoryId)} instead.")]
        public static bool GetDirectoryIdOfChild(string ciphertextChildPath, FileSystemSpecifics specifics, Span<byte> directoryId)
        {
            var directoryIdPath = GetDirectoryIdPathOfParent(ciphertextChildPath, specifics.ContentFolder.Id);
            if (directoryIdPath is null)
                throw new InvalidOperationException("Could not get the Directory ID path.");

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
                throw new IOException($"The data inside the Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }
    }
}