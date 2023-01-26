using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.Dokany.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetFileTime(
            [In] SafeFileHandle hFile,
            [In, Out] ref long lpCreationTime,
            [In, Out] ref long lpLastAccessTime,
            [In, Out] ref long lpLastWriteTime);

        [DllImport(Constants.DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I8)]
        public static extern long DokanNtStatusFromWin32([In][MarshalAs(UnmanagedType.U4)] uint Error);

        [DllImport(Constants.DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong DokanVersion();

        [DllImport(Constants.DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong DokanDriverVersion();

        [DllImport("Shlwapi.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool PathMatchSpec(
            [In] string pszFile,
            [In] string pszSpec);
    }
}
