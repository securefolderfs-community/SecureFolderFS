using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.WinFsp.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport("Shlwapi.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PathMatchSpec(
            [In] string pszFile,
            [In] string pszSpec);
    }
}
