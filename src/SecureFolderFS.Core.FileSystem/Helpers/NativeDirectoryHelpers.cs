using OwlCore.Storage;
using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    public static class NativeDirectoryHelpers
    {
        public static bool GetDirectoryId(string ciphertextChildPath, IFolder contentFolder, Span<byte> directoryId)
        {
            var directoryIdPath = NativePathHelpers.GetDirectoryIdPathOfParent(ciphertextChildPath, contentFolder.Id);
            if (directoryIdPath is null)
                throw new InvalidOperationException("Could not get Directory ID path.");

            // If the Directory ID path is root, return false
            if (directoryIdPath == string.Empty)
                return false;

            // Check directoryId size once we actually get to the reading
            if (directoryId.Length < Constants.DIRECTORY_ID_SIZE)
                throw new ArgumentException($"The {nameof(directoryId)} is of incorrect size: {directoryId.Length}.");

            using var directoryIdStream = File.Open(directoryIdPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var read = directoryIdStream.Read(directoryId);
            if (read < Constants.DIRECTORY_ID_SIZE)
                throw new IOException($"The data inside Directory ID file is of incorrect size: {read}.");

            // The Directory ID is not empty - return true
            return true;
        }
    }
}
