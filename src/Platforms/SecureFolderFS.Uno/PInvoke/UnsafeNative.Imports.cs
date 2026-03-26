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

        public const uint CFNotificationSuspensionBehaviorDeliverImmediately = 4;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LockCallback(IntPtr center, IntPtr observer, IntPtr name, IntPtr obj, IntPtr userInfo);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr CFNotificationCenterGetDistributedCenter();

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr CFStringCreateWithCString(IntPtr allocator, string str, uint encoding);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial void CFNotificationCenterAddObserver(
            IntPtr center,
            IntPtr observer,
            IntPtr callback,
            IntPtr name,
            IntPtr obj,
            uint suspensionBehavior);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial void CFNotificationCenterRemoveObserver(
            IntPtr center,
            IntPtr observer,
            IntPtr name,
            IntPtr obj);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial ulong objc_msgSend_ulong(IntPtr receiver, IntPtr selector);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_ulong(IntPtr receiver, IntPtr selector, ulong value);
        
        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void(IntPtr receiver, IntPtr selector);

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

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool objc_msgSend_bool_long_IntPtr(IntPtr receiver, IntPtr selector, long arg1, IntPtr arg2);

        [LibraryImport("libobjc.dylib", EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_long_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, long arg1, IntPtr arg2, IntPtr arg3);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial void CFRelease(IntPtr cf);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial IntPtr CFRetain(IntPtr cf);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial long CFDataGetLength(IntPtr theData);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial IntPtr CFDataGetBytePtr(IntPtr theData);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial IntPtr CFDataCreate(IntPtr allocator, IntPtr bytes, long length);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial IntPtr CFDictionaryCreate(
            IntPtr allocator,
            IntPtr keys,
            IntPtr values,
            long numValues,
            IntPtr keyCallBacks,
            IntPtr valueCallBacks);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial IntPtr CFNumberCreate(IntPtr allocator, long theType, IntPtr valuePtr);

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
        public static partial IntPtr CFBooleanGetValue(IntPtr boolean);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial IntPtr SecKeyCreateRandomKey(IntPtr parameters, out IntPtr error);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial IntPtr SecKeyCopyPublicKey(IntPtr key);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial IntPtr SecKeyCreateEncryptedData(IntPtr key, IntPtr algorithm, IntPtr plaintext, out IntPtr error);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial IntPtr SecKeyCreateDecryptedData(IntPtr key, IntPtr algorithm, IntPtr ciphertext, out IntPtr error);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial int SecItemAdd(IntPtr attributes, out IntPtr result);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial int SecItemCopyMatching(IntPtr query, out IntPtr result);

        [LibraryImport("/System/Library/Frameworks/Security.framework/Security")]
        public static partial int SecItemDelete(IntPtr query);

        // Well-known CFString constants from Security framework
        [LibraryImport("/System/Library/Frameworks/Security.framework/Security", EntryPoint = "kSecAttrKeyTypeECSECPrimeRandom")]
        public static partial IntPtr GetSecAttrKeyTypeECSECPrimeRandom();

        // Global symbol accessors for Security framework constants
        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", EntryPoint = "kCFBooleanTrue")]
        public static partial IntPtr GetCFBooleanTrue();

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", EntryPoint = "kCFBooleanFalse")]
        public static partial IntPtr GetCFBooleanFalse();

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", EntryPoint = "kCFTypeDictionaryKeyCallBacks")]
        public static partial IntPtr GetCFTypeDictionaryKeyCallBacks();

        [LibraryImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", EntryPoint = "kCFTypeDictionaryValueCallBacks")]
        public static partial IntPtr GetCFTypeDictionaryValueCallBacks();

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
    internal struct CGPoint
    {
        public double X;
        public double Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CGRect
    {
        public double X;
        public double Y;
        public double Width;
        public double Height;
    }
#endif
}
