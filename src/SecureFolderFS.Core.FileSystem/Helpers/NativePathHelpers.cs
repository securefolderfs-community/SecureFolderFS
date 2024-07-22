using System;
using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    public static class NativePathHelpers
    {
        public static string GetCiphertextPath(string cleartextName, FileSystemSpecifics specifics)
        {
            if (specifics.Security.NameCrypt is null)
                return Path.Combine(specifics.ContentFolder.Id, cleartextName);

            var directoryId = DirectoryHelpers.AllocateDirectoryId(cleartextName, specifics);
            var finalPath = specifics.ContentFolder.Id;

            foreach (var namePart in cleartextName.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries))
            {
                var result = NativeDirectoryHelpers.GetDirectoryId(Path.Combine(finalPath, namePart), specifics.ContentFolder, directoryId);
                var ciphertextName = specifics.Security.NameCrypt.EncryptName(namePart, result ? directoryId : ReadOnlySpan<byte>.Empty);

                finalPath = Path.Combine(finalPath, $"{ciphertextName}{Constants.Names.ENCRYPTED_FILE_EXTENSION}");
            }

            return finalPath;
        }

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
            if (fileName.Length == 0 || (fileName.Length == 1 && fileName[0] == Path.DirectorySeparatorChar))
                return PathHelpers.EnsureTrailingPathSeparator(vaultRootPath);

            return Path.Combine(vaultRootPath, PathHelpers.EnsureNoLeadingPathSeparator(fileName));
        }
    }
}
