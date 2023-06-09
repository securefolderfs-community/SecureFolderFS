using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.WebDav.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        public const int CONNECT_TEMPORARY = 4;
        public const int RESOURCETYPE_DISK = 1;

        [DllImport("Mpr.dll", CharSet = CharSet.Unicode)]
        public static extern int WNetAddConnection2(
            [In] NETRESOURCE lpNetResource,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string lpPassword,
            [In] [MarshalAs(UnmanagedType.LPWStr)] string lpUserName,
            uint dwFlags);

        [DllImport("Mpr.dll")]
        public static extern int WNetCancelConnection2(
            [In] string lpName,
            [In] uint dwFlags,
            [In] bool fForce);
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public class NETRESOURCE
    {
        public uint dwScope;
        public uint dwType;
        public uint dwDisplayType;
        public uint dwUsage;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpLocalName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpRemoteName;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpComment;

        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpProvider;
    }
}
