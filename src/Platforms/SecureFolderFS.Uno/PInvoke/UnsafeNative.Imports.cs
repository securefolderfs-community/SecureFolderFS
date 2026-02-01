using System;
using System.Runtime.InteropServices;

namespace SecureFolderFS.Uno.PInvoke
{
    internal static partial class UnsafeNative
    {
#if WINDOWS
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
            [Out] System.Text.StringBuilder lpRemoteName,
            [In, Out] ref int lpnLength);

        [DllImport("user32.dll")]
        public static extern int SendMessageA(
            [In] IntPtr hWnd,
            [In] uint wMsg,
            [In] IntPtr wParam,
            [In] IntPtr lParam);

        /// <summary>
        /// Adds a document to the Shell's list of recently used documents or clears the list.
        /// When pv is IntPtr.Zero, the recent documents list is cleared.
        /// </summary>
        /// <param name="uFlags">The SHARD (Shell Add Recent Document) flag. Use SHARD_PIDL (0x00000001) to clear.</param>
        /// <param name="pv">A pointer to the document path or PIDL. Pass IntPtr.Zero to clear the list.</param>
        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(
            [In] uint uFlags,
            [In] IntPtr pv);
#endif
        
#if __UNO_SKIA_MACOS__
        
        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial ulong objc_msgSend_ulong(IntPtr receiver, IntPtr selector);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_ulong(IntPtr receiver, IntPtr selector, ulong value);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_long(IntPtr receiver, IntPtr selector, long value);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_bool(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.Bool)] bool value);

        [LibraryImport("libobjc.dylib", EntryPoint = "sel_registerName", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr sel_registerName(string name);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr_ulong(IntPtr receiver, IntPtr selector, ulong arg);
        
        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial CGRect objc_msgSend_CGRect(IntPtr receiver, IntPtr selector);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_CGPoint(IntPtr receiver, IntPtr selector, CGPoint point);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_getClass", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr objc_getClass(string className);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);
        
#endif
    }

#if WINDOWS
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
#endif

#if __UNO_SKIA_MACOS__

    [StructLayout(LayoutKind.Sequential)]
    public struct CGPoint
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CGRect
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
    }

#endif
}
