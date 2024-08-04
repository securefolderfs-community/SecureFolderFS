using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    public static class PathHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCoreFile(string fileName)
        {
            return fileName.Contains(Constants.DIRECTORY_ID_FILENAME, StringComparison.Ordinal);
        }
        
        #region Legacy helpers (To be removed)

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureTrailingPathSeparator(string path)
        {
            return path[^1] == Path.DirectorySeparatorChar ? path : path + Path.DirectorySeparatorChar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureNoTrailingPathSeparator(string path)
        {
            return path[^1] == Path.DirectorySeparatorChar ? path.Substring(0, path.Length - 1) : path;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureNoLeadingPathSeparator(string path)
        {
            return path.StartsWith(Path.DirectorySeparatorChar) ? path.Substring(1) : path;
        }

        #endregion

        public static string? GetFreeWindowsMountPath()
        {
            return Enumerable.Range('C', 'Z' - 'C' + 1) // Skip floppy disk drives and system drive
                .Select(item => (char)item)
                .Except(DriveInfo.GetDrives().Select(item => item.Name[0]))
                .Select(item => $"{item}:")
                .FirstOrDefault();
        }
    }
}
