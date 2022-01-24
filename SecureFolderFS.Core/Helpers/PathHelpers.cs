using System;
using System.IO;

namespace SecureFolderFS.Core.Helpers
{
    internal static class PathHelpers
    {
        public static string EnsureTrailingPathSeparator(string path)
        {
            path = path.TrimEnd();

            return path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
        }

        public static string EnsureNoTrailingPathSeparator(string path)
        {
            path = path.TrimEnd();

            return path.EndsWith(Path.DirectorySeparatorChar) ? path.Substring(0, path.Length - 1) : path;
        }

        public static string EnsureNoLeadingPathSeparator(string path)
        {
            path = path.TrimStart();

            return path.StartsWith(Path.DirectorySeparatorChar) ? path.Substring(1) : path;
        }

        public static string EnsurePathIsDirectoryIdOrGetFromParent(string path, string vaultRootPath = null)
        {
            if (path.EndsWith(Path.DirectorySeparatorChar + Constants.DIRECTORY_ID_FILENAME)) // Make sure the path ending part starts with a separator
            {
                return path;
            }
            else
            {
                var parentPath = Path.GetDirectoryName(path);
                if (vaultRootPath != null && parentPath == vaultRootPath)
                {
                    // Parent path is the same as vaultRootPath where the directoryId is we want to get - return empty
                    return string.Empty;
                }
                else
                {
                    return parentPath + Path.DirectorySeparatorChar + Constants.DIRECTORY_ID_FILENAME;
                }
            }
        }

        public static string AppendDirectoryIdPath(string path)
        {
            return Path.Combine(path, Constants.DIRECTORY_ID_FILENAME);
        }

        public static string ConstructFilePathFromVaultRootPath(string vaultRootPath, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return PathHelpers.EnsureTrailingPathSeparator(vaultRootPath);
            }
            else
            {
                if (fileName == "\\")
                {
                    return PathHelpers.EnsureTrailingPathSeparator(vaultRootPath);
                }
                else
                {
                    return Path.Combine(vaultRootPath, PathHelpers.EnsureNoLeadingPathSeparator(fileName));
                }
            }
        }

        public static string AppendExtension(string fileName, string extension)
        {
            return extension.StartsWith('.') ? fileName + extension : $"{fileName}.{extension}";
        }

        public static string RemoveExtension(string fileName, string extension)
        {
            return fileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? fileName.Substring(0, fileName.Length - extension.Length) : fileName;
        }

        public static bool IsCoreFile(string fileName)
        {
            return fileName.Contains(Constants.DIRECTORY_ID_FILENAME);
        }
    }
}
