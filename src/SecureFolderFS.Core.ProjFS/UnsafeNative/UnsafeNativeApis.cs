using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.ProjFS.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DefineDosDevice(
            [In][MarshalAs(UnmanagedType.U4)] uint dwFlags,
            [In][MarshalAs(UnmanagedType.LPWStr)] string lpDeviceName,
            [In][Optional][MarshalAs(UnmanagedType.LPWStr)] string lpTargetPath);
    }
}
