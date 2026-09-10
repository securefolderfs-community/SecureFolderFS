using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths
{
    public static class PathHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsCoreName(string itemName)
        {
            return
                itemName.EndsWith(Constants.Names.SIDECAR_FILE_EXTENSION, StringComparison.OrdinalIgnoreCase) ||
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
            else if (OperatingSystem.IsMacOS())
            {
                return $"{Path.DirectorySeparatorChar}{Path.Combine("Volumes", nameHint)}{Path.DirectorySeparatorChar}";
            }

            return null;
        }

        /// <summary>
        /// Encodes <paramref name="value"/> as NUL-terminated UTF-8 for libc APIs expecting a C string.
        /// </summary>
        /// <remarks>
        /// <see cref="Encoding.GetBytes(string)"/> allocates exactly as many bytes as the encoding needs
        /// and appends no terminator. Handing that array to a <c>byte*</c> binding makes libc scan past
        /// the end of it into whatever follows on the GC heap, and act on a path with arbitrary trailing
        /// bytes, so paths must be terminated explicitly.
        /// </remarks>
        public static byte[] ToNativePath(string value)
        {
            var buffer = new byte[Encoding.UTF8.GetByteCount(value) + 1];
            Encoding.UTF8.GetBytes(value, buffer);

            // The trailing byte is left zero
            return buffer;
        }
    }
}
