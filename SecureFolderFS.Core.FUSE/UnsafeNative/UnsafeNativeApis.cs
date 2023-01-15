using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.FUSE.UnsafeNative
{
    internal static unsafe class UnsafeNativeApis
    {
        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "getxattr")]
        public static extern int GetXAttr(byte *path, byte *name, void *value, int size);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "listxattr")]
        public static extern int ListXAttr(byte *path, byte *list, int size);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "removexattr")]
        public static extern int RemoveXAttr(byte *path, byte *name);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "renameat2")]
        public static extern int RenameAt2(int oldDirFd, byte *oldPath, int newDirFd, byte *newPath, uint flags);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "setxattr")]
        public static extern int SetXAttr(byte *path, byte *name, void *value, int size, int flags);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "opendir")]
        public static extern void* OpenDir(byte *path);

        [DllImport("libc.so.6", SetLastError = true, EntryPoint = "closedir")]
        public static extern int CloseDir(void *fd);
    }
}