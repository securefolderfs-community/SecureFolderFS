using System.IO;

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
            return PathHelpers.EnsureNoLeadingPathSeparator(Path.DirectorySeparatorChar + fullPath.Replace(basePath, string.Empty));
        }
    }
}
