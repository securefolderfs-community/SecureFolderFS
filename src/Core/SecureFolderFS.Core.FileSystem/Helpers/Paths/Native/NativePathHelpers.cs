using System;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    /// <summary>
    /// A set of file system path management helpers that only work in a native environment with unlimited file system access.
    /// </summary>
    public static partial class NativePathHelpers
    {
        public static string MakeRelative(string fullPath, string basePath)
        {
            // Only strip the base path when it is an actual prefix of the full path.
            // Replacing all occurrences could corrupt paths that contain the base path elsewhere
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            if (fullPath.StartsWith(basePath, comparison))
                fullPath = fullPath.Substring(basePath.Length);

            return PathHelpers.EnsureNoLeadingPathSeparator(fullPath);
        }
    }
}
