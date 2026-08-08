using System;
using System.Runtime.InteropServices;
using System.Text;

namespace SecureFolderFS.Uno.PInvoke
{
    internal static partial class UnsafeNative
    {
#if WINDOWS
        public const int CONNECT_TEMPORARY = 4;
        public const int RESOURCETYPE_DISK = 1;
        private const int SM_CMONITORS = 80;
        public const int SW_NORMAL = 1;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWMINIMIZED = 2;
        public const uint WPF_RESTORETOMAXIMIZED = 0x0002;
        public const uint FCSM_ICONFILE = 0x00000010;
        public const uint FCS_FORCEWRITE = 0x00000002;
        public const uint SHCNE_UPDATEITEM = 0x00002000;
        public const uint SHCNF_PATHW = 0x0005;
        public const uint WM_GETMINMAXINFO = 0x0024;
        public const uint WM_DPICHANGED = 0x02E0;

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetDpiForWindow(IntPtr hWnd);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool SetWindowSubclass(
            IntPtr hWnd,
            SUBCLASSPROC pfnSubclass,
            UIntPtr uIdSubclass,
            IntPtr dwRefData);

        [DllImport("comctl32.dll", SetLastError = true)]
        public static extern bool RemoveWindowSubclass(
            IntPtr hWnd,
            SUBCLASSPROC pfnSubclass,
            UIntPtr uIdSubclass);

        [DllImport("comctl32.dll")]
        public static extern IntPtr DefSubclassProc(
            IntPtr hWnd,
            uint uMsg,
            IntPtr wParam,
            IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool EnumDisplayMonitors(
            IntPtr hdc,
            IntPtr lprcClip,
            MonitorEnumDelegate lpfnEnum,
            IntPtr dwData);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFOEX lpmi);

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

        [DllImport("shell32.dll")]
        public static extern void SHAddToRecentDocs(
            [In] uint uFlags,
            [In] IntPtr pv);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int SHGetSetFolderCustomSettings(
            ref SHFOLDERCUSTOMSETTINGS pfcs,
            [MarshalAs(UnmanagedType.LPWStr)] string pszPath,
            uint dwReadWrite);

        [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
        public static extern void SHChangeNotify(
            uint wEventId,
            uint uFlags,
            [MarshalAs(UnmanagedType.LPWStr)] string dwItem1,
            IntPtr dwItem2);

        private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, IntPtr lprcMonitor, IntPtr dwData);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate IntPtr SUBCLASSPROC(
            IntPtr hWnd,
            uint uMsg,
            IntPtr wParam,
            IntPtr lParam,
            UIntPtr uIdSubclass,
            IntPtr dwRefData);
#endif

#if __UNO_SKIA_MACOS__
        public const string LibObjc = "libobjc.dylib";
        public const string SecurityLib = "/System/Library/Frameworks/Security.framework/Security";
        public const string CoreFoundationLib = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

        public const int ErrSecSuccess = 0;
        public const int ErrSecDuplicateItem = -25299;
        public const int ErrSecItemNotFound = -25300;
        public const uint KCfStringEncodingUtf8 = 0x08000100;
        public const uint CFNotificationSuspensionBehaviorDeliverImmediately = 4;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LockCallback(IntPtr center, IntPtr observer, IntPtr name, IntPtr obj, IntPtr userInfo);

        #region Core Foundation
        
        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFDataCreate(IntPtr allocator, IntPtr bytes, long length);

        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFDictionaryCreate(
            IntPtr allocator,
            IntPtr keys,
            IntPtr values,
            long numValues,
            IntPtr keyCallBacks,
            IntPtr valueCallBacks);

        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFNumberCreate(IntPtr allocator, long theType, IntPtr valuePtr);
        
        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFStringCreateWithBytes(IntPtr allocator, byte[] bytes, nint numBytes, uint encoding, byte isExternalRepresentation);

        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFDataCreate(IntPtr allocator, byte[] bytes, nint length);

        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFDictionaryCreate(IntPtr allocator, IntPtr[] keys, IntPtr[] values, nint numValues, IntPtr keyCallBacks, IntPtr valueCallBacks);

        [LibraryImport(CoreFoundationLib)]
        public static partial void CFRelease(IntPtr cf);

        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFDataGetBytePtr(IntPtr theData);

        [LibraryImport(CoreFoundationLib)]
        public static partial nint CFDataGetLength(IntPtr theData);
        
        [LibraryImport(CoreFoundationLib)]
        public static partial IntPtr CFRetain(IntPtr cf);
        
        [LibraryImport(CoreFoundationLib, StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr CFNotificationCenterGetDistributedCenter();

        [LibraryImport(CoreFoundationLib, StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr CFStringCreateWithCString(IntPtr allocator, string str, uint encoding);

        [LibraryImport(CoreFoundationLib)]
        public static partial void CFNotificationCenterAddObserver(
            IntPtr center,
            IntPtr observer,
            IntPtr callback,
            IntPtr name,
            IntPtr obj,
            uint suspensionBehavior);

        [LibraryImport(CoreFoundationLib)]
        public static partial void CFNotificationCenterRemoveObserver(
            IntPtr center,
            IntPtr observer,
            IntPtr name,
            IntPtr obj);
        
        [LibraryImport(CoreFoundationLib, EntryPoint = "kCFBooleanTrue")]
        public static partial IntPtr GetCFBooleanTrue();

        [LibraryImport(CoreFoundationLib, EntryPoint = "kCFBooleanFalse")]
        public static partial IntPtr GetCFBooleanFalse();

        #endregion

        #region Security

        [LibraryImport(SecurityLib)]
        public static partial IntPtr SecKeyCreateRandomKey(IntPtr parameters, out IntPtr error);

        [LibraryImport(SecurityLib)]
        public static partial IntPtr SecKeyCopyPublicKey(IntPtr key);

        [LibraryImport(SecurityLib)]
        public static partial IntPtr SecKeyCreateEncryptedData(IntPtr key, IntPtr algorithm, IntPtr plaintext, out IntPtr error);

        [LibraryImport(SecurityLib)]
        public static partial IntPtr SecKeyCreateDecryptedData(IntPtr key, IntPtr algorithm, IntPtr ciphertext, out IntPtr error);

        [LibraryImport(SecurityLib)]
        public static partial int SecItemAdd(IntPtr attributes, out IntPtr result);

        [LibraryImport(SecurityLib)]
        public static partial int SecItemAdd(IntPtr attributes, IntPtr result);

        [LibraryImport(SecurityLib)]
        public static partial int SecItemCopyMatching(IntPtr query, out IntPtr result);

        [LibraryImport(SecurityLib)]
        public static partial int SecItemUpdate(IntPtr query, IntPtr attributesToUpdate);

        [LibraryImport(SecurityLib)]
        public static partial int SecItemDelete(IntPtr query);        

        #endregion

        #region Objc

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial ulong objc_msgSend_ulong(IntPtr receiver, IntPtr selector);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_ulong(IntPtr receiver, IntPtr selector, ulong value);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void(IntPtr receiver, IntPtr selector);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_long(IntPtr receiver, IntPtr selector, long value);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_bool(IntPtr receiver, IntPtr selector, [MarshalAs(UnmanagedType.U1)] bool value);

        [LibraryImport(LibObjc, EntryPoint = "sel_registerName", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr sel_registerName(string name);

        [LibraryImport(LibObjc, EntryPoint = "objc_getClass", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr objc_getClass(string className);

        [LibraryImport(LibObjc, EntryPoint = "object_getClass")]
        public static partial IntPtr object_getClass(IntPtr obj);

        [LibraryImport(LibObjc, EntryPoint = "objc_allocateClassPair", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr objc_allocateClassPair(IntPtr superclass, string name, nint extraBytes);

        [LibraryImport(LibObjc, EntryPoint = "objc_registerClassPair")]
        public static partial void objc_registerClassPair(IntPtr cls);

        [LibraryImport(LibObjc, EntryPoint = "class_addMethod", StringMarshalling = StringMarshalling.Utf8)]
        [return: MarshalAs(UnmanagedType.U1)]
        public static partial bool class_addMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);

        [LibraryImport(LibObjc, EntryPoint = "class_replaceMethod", StringMarshalling = StringMarshalling.Utf8)]
        public static partial IntPtr class_replaceMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial long objc_msgSend_long(IntPtr receiver, IntPtr selector);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr_double(IntPtr receiver, IntPtr selector, double arg);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr_IntPtr_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, IntPtr arg2, IntPtr arg3);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_CGSize(IntPtr receiver, IntPtr selector, CGSize size);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial IntPtr objc_msgSend_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        [return: MarshalAs(UnmanagedType.U1)]
        public static partial bool objc_msgSend_bool_long_IntPtr(IntPtr receiver, IntPtr selector, long arg1, IntPtr arg2);

        [LibraryImport(LibObjc, EntryPoint = "objc_msgSend")]
        public static partial void objc_msgSend_void_long_IntPtr_IntPtr(IntPtr receiver, IntPtr selector, long arg1, IntPtr arg2, IntPtr arg3);

        #endregion

        public static IntPtr CfString(string value)
        {
            var bytes = Encoding.UTF8.GetBytes(value);
            return CFStringCreateWithBytes(IntPtr.Zero, bytes, bytes.Length, KCfStringEncodingUtf8, 0);
        }

        public static IntPtr CfData(byte[] bytes)
        {
            return CFDataCreate(IntPtr.Zero, bytes, bytes.Length);
        }

#endif
        
#if !__UNO_SKIA_MACOS__ && !WINDOWS

        private const string LibSecret = "libsecret-1.so.0";
        private const string LibGlib = "libglib-2.0.so.0";
        public const string SecretCollectionDefault = "default";

        [LibraryImport(LibSecret, StringMarshalling = StringMarshalling.Utf8)]
        public static partial int secret_password_storev_sync(IntPtr schema, IntPtr attributes, string collection, string label, string password, IntPtr cancellable, out IntPtr error);

        [LibraryImport(LibSecret)]
        public static partial IntPtr secret_password_lookupv_sync(IntPtr schema, IntPtr attributes, IntPtr cancellable, out IntPtr error);

        [LibraryImport(LibSecret)]
        public static partial int secret_password_clearv_sync(IntPtr schema, IntPtr attributes, IntPtr cancellable, out IntPtr error);

        [LibraryImport(LibSecret)]
        public static partial void secret_password_free(IntPtr password);

        [LibraryImport(LibGlib)]
        public static partial IntPtr g_hash_table_new(IntPtr hashFunc, IntPtr keyEqualFunc);

        [LibraryImport(LibGlib)]
        public static partial void g_hash_table_insert(IntPtr hashTable, IntPtr key, IntPtr value);

        [LibraryImport(LibGlib)]
        public static partial void g_hash_table_unref(IntPtr hashTable);

        [LibraryImport(LibGlib)]
        public static partial void g_error_free(IntPtr error);
        
        public static class GlibFunctions
        {
            internal static readonly IntPtr StrHash;
            internal static readonly IntPtr StrEqual;

            static GlibFunctions()
            {
                var glib = NativeLibrary.Load(LibGlib);
                StrHash = NativeLibrary.GetExport(glib, "g_str_hash");
                StrEqual = NativeLibrary.GetExport(glib, "g_str_equal");
            }
        }

        /// <summary>
        /// Owns a GHashTable of string attributes ({"key": value} or empty) plus the unmanaged
        /// strings inserted into it (libsecret copies what it needs during the sync calls).
        /// </summary>
        public readonly struct SecretAttributes : IDisposable
        {
            internal IntPtr Handle { get; }
            private readonly IntPtr _keyPtr;
            private readonly IntPtr _valuePtr;

            internal SecretAttributes(string? key)
            {
                Handle = g_hash_table_new(GlibFunctions.StrHash, GlibFunctions.StrEqual);
                if (key is not null)
                {
                    _keyPtr = Marshal.StringToHGlobalAnsi("Key");
                    _valuePtr = Marshal.StringToHGlobalAnsi(key);
                    g_hash_table_insert(Handle, _keyPtr, _valuePtr);
                }
                else
                {
                    _keyPtr = IntPtr.Zero;
                    _valuePtr = IntPtr.Zero;
                }
            }

            public void Dispose()
            {
                if (Handle != IntPtr.Zero)
                    g_hash_table_unref(Handle);
                if (_keyPtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(_keyPtr);
                if (_valuePtr != IntPtr.Zero)
                    Marshal.FreeHGlobal(_valuePtr);
            }
        }
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

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct MONITORINFOEX
    {
        public uint cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public uint dwFlags;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)] // CCHDEVICENAME
        public string szDevice;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

        public bool IsEmpty => Left == 0 && Top == 0 && Right == 0 && Bottom == 0;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct WINDOWPLACEMENT
    {
        public uint length;
        public uint flags;
        public int showCmd;
        public POINT ptMinPosition;
        public POINT ptMaxPosition;
        public RECT rcNormalPosition;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct POINT
    {
        public int X;
        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct MINMAXINFO
    {
        public POINT ptReserved;
        public POINT ptMaxSize;
        public POINT ptMaxPosition;
        public POINT ptMinTrackSize;
        public POINT ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct SHFOLDERCUSTOMSETTINGS
    {
        public uint dwSize;
        public uint dwMask;
        public IntPtr pvid;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszWebViewTemplate;
        public uint cchWebViewTemplate;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszWebViewTemplateVersion;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszInfoTip;
        public uint cchInfoTip;
        public IntPtr pclsid;
        public uint dwFlags;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszIconFile;
        public uint cchIconFile;
        public int iIconIndex;
        [MarshalAs(UnmanagedType.LPWStr)] public string pszLogo;
        public uint cchLogo;
    }
#endif

#if __UNO_SKIA_MACOS__
    /// <summary>
    /// Lazily-resolved CoreFoundation/Security constants. kSec* symbols are exported CFStringRef
    /// variables (dereference the export); the dictionary callback symbols are the structs
    /// themselves (use the export address directly).
    /// </summary>
    public static class MacOsConstants
    {
        internal static readonly IntPtr SecClass;
        internal static readonly IntPtr SecClassGenericPassword;
        internal static readonly IntPtr SecAttrService;
        internal static readonly IntPtr SecAttrAccount;
        internal static readonly IntPtr SecValueData;
        internal static readonly IntPtr SecReturnData;
        internal static readonly IntPtr SecMatchLimit;
        internal static readonly IntPtr SecMatchLimitOne;
        internal static readonly IntPtr CfBooleanTrue;
        internal static readonly IntPtr TypeDictionaryKeyCallBacks;
        internal static readonly IntPtr TypeDictionaryValueCallBacks;

        static MacOsConstants()
        {
            var security = NativeLibrary.Load(UnsafeNative.SecurityLib);
            var coreFoundation = NativeLibrary.Load(UnsafeNative.CoreFoundationLib);

            SecClass = Deref(security, "kSecClass");
            SecClassGenericPassword = Deref(security, "kSecClassGenericPassword");
            SecAttrService = Deref(security, "kSecAttrService");
            SecAttrAccount = Deref(security, "kSecAttrAccount");
            SecValueData = Deref(security, "kSecValueData");
            SecReturnData = Deref(security, "kSecReturnData");
            SecMatchLimit = Deref(security, "kSecMatchLimit");
            SecMatchLimitOne = Deref(security, "kSecMatchLimitOne");
            CfBooleanTrue = Deref(coreFoundation, "kCFBooleanTrue");
            TypeDictionaryKeyCallBacks = NativeLibrary.GetExport(coreFoundation, "kCFTypeDictionaryKeyCallBacks");
            TypeDictionaryValueCallBacks = NativeLibrary.GetExport(coreFoundation, "kCFTypeDictionaryValueCallBacks");
        }

        private static IntPtr Deref(IntPtr library, string symbol)
            => Marshal.ReadIntPtr(NativeLibrary.GetExport(library, symbol));
    }
    
    /// <summary>
    /// Owns a CFDictionary and the CF value objects passed into it (keys are shared kSec* constants and must not be released).
    /// </summary>
    public readonly struct CfDictionary : IDisposable
    {
        internal IntPtr Handle { get; }
        private readonly IntPtr[] _ownedValues;

        internal CfDictionary(params (IntPtr key, IntPtr value)[] entries)
        {
            var keys = new IntPtr[entries.Length];
            var values = new IntPtr[entries.Length];
            var owned = new IntPtr[entries.Length];

            for (var i = 0; i < entries.Length; i++)
            {
                keys[i] = entries[i].key;
                values[i] = entries[i].value;

                // Constants (booleans, match limits) are process-wide singletons — don't release them.
                owned[i] = values[i] != MacOsConstants.CfBooleanTrue && values[i] != MacOsConstants.SecMatchLimitOne &&
                           values[i] != MacOsConstants.SecClassGenericPassword
                    ? values[i]
                    : IntPtr.Zero;
            }

            _ownedValues = owned;
            Handle = UnsafeNative.CFDictionaryCreate(
                IntPtr.Zero, keys, values, entries.Length,
                MacOsConstants.TypeDictionaryKeyCallBacks, MacOsConstants.TypeDictionaryValueCallBacks);
        }

        public void Dispose()
        {
            if (Handle != IntPtr.Zero)
                UnsafeNative.CFRelease(Handle);

            foreach (var value in _ownedValues)
            {
                if (value != IntPtr.Zero)
                    UnsafeNative.CFRelease(value);
            }
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct CGSize
    {
        public double Width;
        public double Height;
    }
#endif
}
