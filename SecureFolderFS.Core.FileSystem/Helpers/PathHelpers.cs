using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem.Helpers
{
    public static class PathHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureTrailingPathSeparator(string path)
        {
            return path.EndsWith(Path.DirectorySeparatorChar) ? path : path + Path.DirectorySeparatorChar;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string EnsureNoLeadingPathSeparator(string path)
        {
            return path.StartsWith(Path.DirectorySeparatorChar) ? path.Substring(1) : path;
        }

        public static string PathFromVaultRoot(string fileName, string vaultRootPath)
        {
            if (string.IsNullOrEmpty(fileName)|| (fileName.Length == 1 && fileName[0] == Path.DirectorySeparatorChar))
                return EnsureTrailingPathSeparator(vaultRootPath);

            return Path.Combine(vaultRootPath, EnsureNoLeadingPathSeparator(fileName));
        }


    }
}
