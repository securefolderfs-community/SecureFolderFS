using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths
{
    public static class PathHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCoreName(string itemName)
        {
            return
                itemName.Contains(Constants.Names.DIRECTORY_ID_FILENAME, StringComparison.OrdinalIgnoreCase) ||
                itemName.Contains(Constants.Names.RECYCLE_BIN_NAME, StringComparison.OrdinalIgnoreCase) ||
                itemName.Contains(Constants.Names.RECYCLE_BIN_CONFIGURATION_FILENAME, StringComparison.OrdinalIgnoreCase);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureNoLeadingPathSeparator(string path)
        {
            return path.StartsWith(Path.DirectorySeparatorChar) ? path.Substring(1) : path;
        }

        public static string? GetFreeMountPath(string nameHint)
        {
            if (OperatingSystem.IsWindows())
            {
                return Enumerable.Range('C', 'Z' - 'C' + 1) // Skip floppy disk drives and system drive
                    .Select(item => (char)item)
                    .Except(DriveInfo.GetDrives().Select(item => item.Name[0]))
                    .Select(item => $"{item}:")
                    .FirstOrDefault();
            }
            else if (OperatingSystem.IsMacCatalyst())
            {
                return $"{Path.DirectorySeparatorChar}{Path.Combine("Volumes", nameHint)}{Path.DirectorySeparatorChar}";
            }

            return null;
        }
    }
}
