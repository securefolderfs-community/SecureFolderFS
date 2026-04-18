using System.IO;

namespace SecureFolderFS.Core.FileSystem.Helpers.Paths.Native
{
    /// <summary>
    /// A set of file system path management helpers that only work in a native environment with unlimited file system access.
    /// </summary>
    public static partial class NativePathHelpers
    {
        public static string MakeRelative(string fullPath, string basePath)
        {
            return PathHelpers.EnsureNoLeadingPathSeparator(Path.DirectorySeparatorChar + fullPath.Replace(basePath, string.Empty));
        }
    }
}
