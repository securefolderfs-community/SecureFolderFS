using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace SecureFolderFS.Core.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetFileTime([In] SafeFileHandle hFile, [In, Out] ref long lpCreationTime, [In, Out] ref long lpLastAccessTime, [In, Out] ref long lpLastWriteTime);

        [DllImport("kernel32.dll", CallingConvention = CallingConvention.StdCall, SetLastError = true)]
        public static extern IntPtr CreateFile(
            [In][MarshalAs(UnmanagedType.LPWStr)] string lpFileName,
            [In][MarshalAs(UnmanagedType.U4)] uint dwDesiredAccess,
            [In][MarshalAs(UnmanagedType.U4)] uint dwShareMode,
            [In, Optional] IntPtr lpSecurityAttributes,
            [In][MarshalAs(UnmanagedType.U4)] uint dwCreationDisposition,
            [In][MarshalAs(UnmanagedType.U4)] uint dwFlagsAndAttributes,
            [In, Optional] IntPtr hTemplateFile);

        [DllImport("kernel32.dll", SetLastError = true)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool CloseHandle([In] IntPtr hObject);

        [DllImport("kernel32.dll")]
        public static extern void RtlZeroMemory([Out] IntPtr ptr, [In] UIntPtr cnt);

        [DllImport("dokan1.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.I8)]
        public static extern long DokanNtStatusFromWin32([In][MarshalAs(UnmanagedType.U4)] uint Error);

        [DllImport("dokan1.dll", ExactSpelling = true)] // TODO: When v2 releases, change to dokan2.dll
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong DokanVersion();

        [DllImport("dokan1.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong DokanDriverVersion();
    }
}
