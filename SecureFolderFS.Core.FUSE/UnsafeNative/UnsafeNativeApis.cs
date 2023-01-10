using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.FUSE.UnsafeNative
{
    internal sealed class UnsafeNativeApis
    {
        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "renameat2")]
        public static extern unsafe int RenameAt2(int oldDirFd, byte *oldPath, int newDirFd, byte *newPath, uint flags);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "opendir")]
        public static extern unsafe int OpenDir(byte *path);
    }
}