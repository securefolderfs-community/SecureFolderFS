using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.FUSE.UnsafeNative
{
    internal sealed class UnsafeNativeApis
    {
        [DllImport("libc.so.6", SetLastError = true)]
        public static extern unsafe int rename(byte *oldPath, byte *newPath);
    }
}