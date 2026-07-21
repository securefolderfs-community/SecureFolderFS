using System;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    /// <summary>
    /// A set of file system path management helpers that only work in a native environment with unlimited file system access.
    /// </summary>
    public static partial class NativePathHelpers
    {
        /// <summary>
        /// Makes the provided <paramref name="fullPath"/> relative to the specified <paramref name="basePath"/>.
        /// </summary>
        /// <param name="fullPath">The full path to convert.</param>
        /// <param name="basePath">The base path to make the full path relative to.</param>
        /// <returns>A relative path with a leading directory separator.</returns>
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
