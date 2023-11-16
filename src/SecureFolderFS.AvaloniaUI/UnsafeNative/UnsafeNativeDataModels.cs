using System;
using System.Runtime.InteropServices;

namespace SecureFolderFS.AvaloniaUI.UnsafeNative
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    internal ref struct utsname
    {
        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 65)]
        public byte[] sysname;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 65)]
        public byte[] nodename;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 65)]
        public byte[] release;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 65)]
        public byte[] version;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 65)]
        public byte[] machine;

        [MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 65)]
        public byte[] domainname;
    }
}