using System.Runtime.InteropServices;

namespace SecureFolderFS.AvaloniaUI.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport("libc.so.6", SetLastError = true)]
        internal static extern int uname(ref utsname buf);
    }
}