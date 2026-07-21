#if __UNO_SKIA_MACOS__
using System;
using System.IO;
using System.Runtime.InteropServices;
using SecureFolderFS.Uno.PInvoke;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    /// <summary>
    /// Manages an NSStatusItem in the macOS system menu bar (status bar) with common application actions.
    /// Also handles Dock icon reopen requests so that clicking the Dock icon restores a hidden main window.
    /// </summary>
    internal sealed class MacOsStatusBarHelper
    {
        private const double NSVariableStatusItemLength = -1d;
        private const long SHOW_APP_TAG = 1;
        private const long LOCK_ALL_TAG = 2;
        private const long EXIT_APP_TAG = 3;

        // Rooted delegates to prevent the GC from collecting the native callbacks
        private static MenuItemCallback? _menuItemCallback;
        private static ReopenCallback? _reopenCallback;
        private static MacOsStatusBarHelper? _instance;

        private IntPtr _statusItem;
        private IntPtr _lockAllItem;
        private IntPtr _target;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void MenuItemCallback(IntPtr self, IntPtr cmd, IntPtr sender);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.U1)]
        private delegate bool ReopenCallback(IntPtr self, IntPtr cmd, IntPtr application, [MarshalAs(UnmanagedType.U1)] bool hasVisibleWindows);

        /// <summary>
        /// Gets or sets the action invoked when the user requests to show the application window.
        /// </summary>
        public Action? ShowAppRequested { get; set; }

        /// <summary>
        /// Gets or sets the action invoked when the user requests to lock all vaults.
        /// </summary>
        public Action? LockAllRequested { get; set; }

        /// <summary>
        /// Gets or sets the action invoked when the user requests to exit the application.
        /// </summary>
        public Action? ExitAppRequested { get; set; }

        private MacOsStatusBarHelper()
        {
        }

        /// <summary>
        /// Creates the status bar item with the provided menu item titles, or returns the existing instance.
        /// </summary>
        /// <remarks>
        /// Must be called on the main thread.
        /// </remarks>
        /// <param name="iconPath">The path to the icon file shown in the status bar.</param>
        /// <param name="showAppTitle">The title of the menu item that shows the app window.</param>
        /// <param name="lockAllTitle">The title of the menu item that locks all vaults.</param>
        /// <param name="exitAppTitle">The title of the menu item that exits the app.</param>
        /// <returns>The <see cref="MacOsStatusBarHelper"/> instance, or null if initialization failed.</returns>
        public static MacOsStatusBarHelper? GetOrCreate(string iconPath, string showAppTitle, string lockAllTitle, string exitAppTitle)
        {
            if (_instance is not null)
                return _instance;

            if (!OperatingSystem.IsMacOS())
                return null;

            try
            {
                var instance = new MacOsStatusBarHelper();
                if (!instance.Initialize(iconPath, showAppTitle, lockAllTitle, exitAppTitle))
                    return null;

                _instance = instance;
                return instance;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Enables or disables the 'Lock All' menu item.
        /// </summary>
        /// <param name="isEnabled">Whether the menu item should be enabled.</param>
        public void SetLockAllEnabled(bool isEnabled)
        {
            try
            {
                if (_lockAllItem != IntPtr.Zero)
                    objc_msgSend_void_bool(_lockAllItem, sel_registerName("setEnabled:"), isEnabled);
            }
            catch
            {
            }
        }

        private bool Initialize(string iconPath, string showAppTitle, string lockAllTitle, string exitAppTitle)
        {
            // Create the Objective-C target class that receives menu item actions
            _target = CreateTargetInstance();
            if (_target == IntPtr.Zero)
                return false;

            // Get [NSStatusBar systemStatusBar]
            var statusBarClass = objc_getClass("NSStatusBar");
            var systemStatusBar = objc_msgSend_IntPtr(statusBarClass, sel_registerName("systemStatusBar"));
            if (systemStatusBar == IntPtr.Zero)
                return false;

            // Create the status item and retain it (the status bar does not keep a strong reference)
            _statusItem = objc_msgSend_IntPtr_double(systemStatusBar, sel_registerName("statusItemWithLength:"), NSVariableStatusItemLength);
            if (_statusItem == IntPtr.Zero)
                return false;

            CFRetain(_statusItem);

            // Configure the status item button with the app icon
            var button = objc_msgSend_IntPtr(_statusItem, sel_registerName("button"));
            if (button != IntPtr.Zero)
            {
                var image = CreateStatusBarImage(iconPath);
                if (image != IntPtr.Zero)
                    objc_msgSend_void_IntPtr(button, sel_registerName("setImage:"), image);
                else
                    objc_msgSend_void_IntPtr(button, sel_registerName("setTitle:"), CreateNSString(nameof(SecureFolderFS)));

                objc_msgSend_void_IntPtr(button, sel_registerName("setToolTip:"), CreateNSString(nameof(SecureFolderFS)));
            }

            // Create the menu: Show App | Lock All | --- | Exit App
            var menu = AllocInit("NSMenu");
            objc_msgSend_void_bool(menu, sel_registerName("setAutoenablesItems:"), false);

            AddMenuItem(menu, showAppTitle, SHOW_APP_TAG);
            _lockAllItem = AddMenuItem(menu, lockAllTitle, LOCK_ALL_TAG);

            var separatorItem = objc_msgSend_IntPtr(objc_getClass("NSMenuItem"), sel_registerName("separatorItem"));
            objc_msgSend_void_IntPtr(menu, sel_registerName("addItem:"), separatorItem);

            AddMenuItem(menu, exitAppTitle, EXIT_APP_TAG);

            objc_msgSend_void_IntPtr(_statusItem, sel_registerName("setMenu:"), menu);
            SetLockAllEnabled(false);

            // Restore the window when the Dock icon is clicked while the window is hidden
            RegisterDockReopenHandler();

            return true;
        }

        private IntPtr AddMenuItem(IntPtr menu, string title, long tag)
        {
            // Create [[NSMenuItem alloc] initWithTitle:action:keyEquivalent:]
            var menuItemClass = objc_getClass("NSMenuItem");
            var menuItem = objc_msgSend_IntPtr(menuItemClass, sel_registerName("alloc"));
            menuItem = objc_msgSend_IntPtr_IntPtr_IntPtr_IntPtr(
                menuItem,
                sel_registerName("initWithTitle:action:keyEquivalent:"),
                CreateNSString(title),
                sel_registerName("menuItemInvoked:"),
                CreateNSString(string.Empty));

            objc_msgSend_void_IntPtr(menuItem, sel_registerName("setTarget:"), _target);
            objc_msgSend_void_long(menuItem, sel_registerName("setTag:"), tag);
            objc_msgSend_void_IntPtr(menu, sel_registerName("addItem:"), menuItem);

            return menuItem;
        }

        private static IntPtr CreateTargetInstance()
        {
            // Register the Objective-C class that dispatches menu actions back to managed code
            var targetClass = objc_allocateClassPair(objc_getClass("NSObject"), "SFFSStatusBarTarget", 0);
            if (targetClass == IntPtr.Zero)
                return IntPtr.Zero;

            _menuItemCallback = MenuItemInvoked;
            var imp = Marshal.GetFunctionPointerForDelegate(_menuItemCallback);
            if (!class_addMethod(targetClass, sel_registerName("menuItemInvoked:"), imp, "v@:@"))
                return IntPtr.Zero;

            objc_registerClassPair(targetClass);

            return AllocInit("SFFSStatusBarTarget");
        }

        private static void RegisterDockReopenHandler()
        {
            try
            {
                // Get the class of [[NSApplication sharedApplication] delegate]
                var nsApplicationClass = objc_getClass("NSApplication");
                var sharedApplication = objc_msgSend_IntPtr(nsApplicationClass, sel_registerName("sharedApplication"));
                var appDelegate = objc_msgSend_IntPtr(sharedApplication, sel_registerName("delegate"));
                if (appDelegate == IntPtr.Zero)
                    return;

                // Uno's application delegate does not implement applicationShouldHandleReopen:hasVisibleWindows:,
                // so add it to handle Dock icon clicks (class_addMethod is a no-op if it ever becomes implemented)
                _reopenCallback = ApplicationShouldHandleReopen;
                var imp = Marshal.GetFunctionPointerForDelegate(_reopenCallback);
                class_addMethod(object_getClass(appDelegate), sel_registerName("applicationShouldHandleReopen:hasVisibleWindows:"), imp, "B@:@B");
            }
            catch
            {
            }
        }

        private static void MenuItemInvoked(IntPtr self, IntPtr cmd, IntPtr sender)
        {
            try
            {
                var tag = objc_msgSend_long(sender, sel_registerName("tag"));
                switch (tag)
                {
                    case SHOW_APP_TAG:
                        _instance?.ShowAppRequested?.Invoke();
                        break;

                    case LOCK_ALL_TAG:
                        _instance?.LockAllRequested?.Invoke();
                        break;

                    case EXIT_APP_TAG:
                        _instance?.ExitAppRequested?.Invoke();
                        break;
                }
            }
            catch
            {
                // Exceptions must not cross the native boundary
            }
        }

        private static bool ApplicationShouldHandleReopen(IntPtr self, IntPtr cmd, IntPtr application, bool hasVisibleWindows)
        {
            try
            {
                if (!hasVisibleWindows)
                    _instance?.ShowAppRequested?.Invoke();
            }
            catch
            {
                // Exceptions must not cross the native boundary
            }

            return hasVisibleWindows;
        }

        private static IntPtr CreateStatusBarImage(string iconPath)
        {
            if (!File.Exists(iconPath))
                return IntPtr.Zero;

            // Create [[NSImage alloc] initWithContentsOfFile:path]
            var image = objc_msgSend_IntPtr(objc_getClass("NSImage"), sel_registerName("alloc"));
            image = objc_msgSend_IntPtr_IntPtr(image, sel_registerName("initWithContentsOfFile:"), CreateNSString(iconPath));
            if (image == IntPtr.Zero)
                return IntPtr.Zero;

            // Scale down to the standard menu bar icon size
            objc_msgSend_void_CGSize(image, sel_registerName("setSize:"), new CGSize() { Width = 18d, Height = 18d });

            return image;
        }

        private static IntPtr AllocInit(string className)
        {
            var instance = objc_msgSend_IntPtr(objc_getClass(className), sel_registerName("alloc"));
            return objc_msgSend_IntPtr(instance, sel_registerName("init"));
        }

        private static IntPtr CreateNSString(string value)
        {
            var valuePtr = Marshal.StringToCoTaskMemUTF8(value);
            try
            {
                return objc_msgSend_IntPtr_IntPtr(objc_getClass("NSString"), sel_registerName("stringWithUTF8String:"), valuePtr);
            }
            finally
            {
                Marshal.FreeCoTaskMem(valuePtr);
            }
        }
    }
}
#endif
