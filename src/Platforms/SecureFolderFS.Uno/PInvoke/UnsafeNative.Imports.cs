using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SecureFolderFS.Uno.PInvoke
{
    internal static partial class UnsafeNative
    {
        public const int CONNECT_TEMPORARY = 4;
        public const int RESOURCETYPE_DISK = 1;

        [DllImport("Mpr.dll")]
        public static extern int WNetAddConnection2(
            [In] NETRESOURCE lpNetResource,
            [In] string lpPassword,
            [In] string lpUserName,
            [In] uint dwFlags);

        [DllImport("Mpr.dll")]
        public static extern int WNetCancelConnection2(
            [In] string lpName,
            [In] uint dwFlags,
            [In] bool fForce);

        [DllImport("Mpr.dll")]
        public static extern int WNetGetConnection(
            [In] string lpLocalName,
            [Out] StringBuilder lpRemoteName,
            [In, Out] ref int lpnLength);

        [DllImport("user32.dll")]
        public static extern int SendMessageA(
            [In] IntPtr hWnd,
            [In] uint wMsg,
            [In] IntPtr wParam,
            [In] IntPtr lParam);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class NETRESOURCE
    {
        public uint dwScope;
        public uint dwType;
        public uint dwDisplayType;
        public uint dwUsage;
        public string lpLocalName = null!;
        public string lpRemoteName = null!;
        public string lpComment = null!;
        public string lpProvider = null!;
    }
}
