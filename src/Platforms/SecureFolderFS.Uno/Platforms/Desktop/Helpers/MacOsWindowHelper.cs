#if __UNO_SKIA_MACOS__ || __MACCATALYST__
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using Uno.UI.Xaml;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    /// <summary>
    /// Provides helper methods for configuring macOS title bar to extend content into the title bar area
    /// while preserving the native window chrome (traffic light buttons and rounded corners).
    /// </summary>
    internal static class MacOsWindowHelper
    {
        // NSWindowStyleMask values (for reference)
        private const ulong NSWindowStyleMaskFullSizeContentView = 1 << 15;

        // NSWindowTitleVisibility values
        private const long NSWindowTitleHidden = 1;

        // Rooted delegates and captured state for the native close interceptor.
        // These must remain referenced for the lifetime of the app so the GC does not
        // collect the callbacks that AppKit invokes.
        private static WindowShouldCloseDelegate? _closeOverride;
        private static TerminateDelegate? _terminateOverride;
        private static IntPtr _mainWindowHandle;
        private static IntPtr _originalWindowShouldClose;
        private static Func<bool>? _shouldHideOnClose;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool WindowShouldCloseDelegate(IntPtr self, IntPtr cmd, IntPtr sender);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool TerminateDelegate(IntPtr self, IntPtr cmd, IntPtr sender);

        /// <summary>
        /// Intercepts the main window's native <c>windowShouldClose:</c> so that closing it via the
        /// traffic light button hides the window instead of destroying it, keeping the app running in
        /// the background. This bypasses Uno's managed close-cancellation, which is a no-op on the
        /// macOS Skia host because <c>NativeWindowFactory.SupportsClosingCancellation</c> resolves to false.
        /// </summary>
        /// <param name="window">The main window to intercept.</param>
        /// <param name="shouldHideOnClose">
        /// A predicate evaluated on each close attempt. When it returns true, the window is hidden and
        /// the close is cancelled; otherwise the close proceeds normally (e.g. when quitting the app).
        /// </param>
        /// <returns>True if the interceptor was installed; otherwise, false.</returns>
        public static bool InstallMainWindowCloseInterceptor(Window window, Func<bool> shouldHideOnClose)
        {
            if (!OperatingSystem.IsMacOS())
                return false;

            try
            {
                var handle = GetNativeWindowHandle(window);
                if (handle == IntPtr.Zero)
                    return false;

                var windowClass = object_getClass(handle);
                if (windowClass == IntPtr.Zero)
                    return false;

                _mainWindowHandle = handle;
                _shouldHideOnClose = shouldHideOnClose;
                _closeOverride = WindowShouldCloseOverride;

                var imp = Marshal.GetFunctionPointerForDelegate(_closeOverride);

                // Replacing on the class affects every NSWindow of this type, so the override forwards
                // to the captured original implementation for any window that is not the main window.
                _originalWindowShouldClose = class_replaceMethod(windowClass, sel_registerName("windowShouldClose:"), imp, "c@:@");

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Overrides the application delegate's <c>applicationShouldTerminateAfterLastWindowClosed:</c>
        /// to return false, so the app keeps running in the background when the last window is closed
        /// (standard behavior for a menu bar app). Acts as a safety net alongside the close interceptor.
        /// </summary>
        /// <returns>True if the override was installed; otherwise, false.</returns>
        public static bool PreventTerminationOnLastWindowClose()
        {
            if (!OperatingSystem.IsMacOS())
                return false;

            try
            {
                var nsApplicationClass = objc_getClass("NSApplication");
                var sharedApplication = objc_msgSend_IntPtr(nsApplicationClass, sel_registerName("sharedApplication"));
                var appDelegate = objc_msgSend_IntPtr(sharedApplication, sel_registerName("delegate"));
                if (appDelegate == IntPtr.Zero)
                    return false;

                _terminateOverride = static (_, _, _) => false;
                var imp = Marshal.GetFunctionPointerForDelegate(_terminateOverride);
                class_replaceMethod(object_getClass(appDelegate), sel_registerName("applicationShouldTerminateAfterLastWindowClosed:"), imp, "c@:@");

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool WindowShouldCloseOverride(IntPtr self, IntPtr cmd, IntPtr sender)
        {
            try
            {
                if (self == _mainWindowHandle && _shouldHideOnClose?.Invoke() == true)
                {
                    // Hide the window instead of closing it; the app stays alive in the background.
                    objc_msgSend_void_IntPtr(self, sel_registerName("orderOut:"), IntPtr.Zero);
                    return false;
                }
            }
            catch
            {
                // Exceptions must not cross the native boundary
            }

            // Forward to Uno's original implementation for non-main windows or an actual close request.
            if (_originalWindowShouldClose != IntPtr.Zero)
            {
                var original = Marshal.GetDelegateForFunctionPointer<WindowShouldCloseDelegate>(_originalWindowShouldClose);
                return original(self, cmd, sender);
            }

            return true;
        }

        /// <summary>
        /// Configures the window to extend content into the title bar while preserving
        /// the native macOS window chrome (traffic light buttons and rounded corners).
        /// </summary>
        /// <param name="window">The window to configure.</param>
        /// <returns>True if the configuration was successful; otherwise, false.</returns>
        public static bool ConfigureFullSizeContentView(Window window)
        {
            if (!OperatingSystem.IsMacOS())
                return false;

            try
            {
                // Get the native window handle
                var nativeWindow = GetNativeWindowHandle(window);
                if (nativeWindow == IntPtr.Zero)
                    return false;

                // Get the current style mask
                var currentStyleMask = GetStyleMask(nativeWindow);

                // Add FullSizeContentView style while keeping other styles
                var newStyleMask = currentStyleMask | NSWindowStyleMaskFullSizeContentView;
                SetStyleMask(nativeWindow, newStyleMask);

                // Hide the title text but keep the title bar area
                SetTitleVisibility(nativeWindow, NSWindowTitleHidden);

                // Make the title bar transparent so content shows through
                SetTitlebarAppearsTransparent(nativeWindow, true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the traffic light buttons inset (the space needed for close/minimize/maximize buttons).
        /// </summary>
        /// <returns>A tuple containing (left padding, top padding) for the traffic light buttons.</returns>
        public static (double Left, double Top) GetTrafficLightButtonsInset()
        {
            return (64, 0);
        }

        /// <summary>
        /// Centers the specified window on the screen.
        /// </summary>
        /// <param name="window">The window to center.</param>
        /// <returns>True if the window was successfully centered; otherwise, false.</returns>
        public static bool CenterWindow(Window window)
        {
            if (!OperatingSystem.IsMacOS())
                return false;

            try
            {
                var nsWindow = GetNativeWindowHandle(window);
                if (nsWindow == IntPtr.Zero)
                    return false;

                // Call [nsWindow center]
                var selector = sel_registerName("center");
                objc_msgSend_void(nsWindow, selector);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Hides the specified window without closing it, keeping the application running in the background.
        /// </summary>
        /// <param name="window">The window to hide.</param>
        /// <returns>True if the window was successfully hidden; otherwise, false.</returns>
        public static bool HideWindow(Window window)
        {
            if (!OperatingSystem.IsMacOS())
                return false;

            try
            {
                var nsWindow = GetNativeWindowHandle(window);
                if (nsWindow == IntPtr.Zero)
                    return false;

                // Call [nsWindow orderOut:nil]
                var selector = sel_registerName("orderOut:");
                objc_msgSend_void_IntPtr(nsWindow, selector, IntPtr.Zero);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Shows a previously hidden window and brings the application to the foreground.
        /// </summary>
        /// <param name="window">The window to show.</param>
        /// <returns>True if the window was successfully shown; otherwise, false.</returns>
        public static bool ShowWindow(Window window)
        {
            if (!OperatingSystem.IsMacOS())
                return false;

            try
            {
                var nsWindow = GetNativeWindowHandle(window);
                if (nsWindow == IntPtr.Zero)
                    return false;

                // Call [nsWindow makeKeyAndOrderFront:nil]
                objc_msgSend_void_IntPtr(nsWindow, sel_registerName("makeKeyAndOrderFront:"), IntPtr.Zero);

                // Call [[NSApplication sharedApplication] activateIgnoringOtherApps:YES]
                var nsApplicationClass = objc_getClass("NSApplication");
                var sharedApplication = objc_msgSend_IntPtr(nsApplicationClass, sel_registerName("sharedApplication"));
                objc_msgSend_void_bool(sharedApplication, sel_registerName("activateIgnoringOtherApps:"), true);

                return true;
            }
            catch
            {
                return false;
            }
        }

        private static IntPtr GetNativeWindowHandle(Window window)
        {
            try
            {
                // Get the MacOSWindowNative instance
                var nativeWindowWrapper = WindowHelper.GetNativeWindow(window);
                if (nativeWindowWrapper == null)
                    return IntPtr.Zero;

                // Use reflection to access the internal 'Handle' property
                var handleProperty = nativeWindowWrapper.GetType().GetProperty("Handle",
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

                var handleValue = handleProperty?.GetValue(nativeWindowWrapper);
                if (handleValue is nint handle)
                    return handle;

                return IntPtr.Zero;
            }
            catch (Exception)
            {
                return IntPtr.Zero;
            }
        }

        private static ulong GetStyleMask(IntPtr nsWindow)
        {
            var selector = sel_registerName("styleMask");
            return objc_msgSend_ulong(nsWindow, selector);
        }

        private static void SetStyleMask(IntPtr nsWindow, ulong styleMask)
        {
            var selector = sel_registerName("setStyleMask:");
            objc_msgSend_void_ulong(nsWindow, selector, styleMask);
        }

        private static void SetTitleVisibility(IntPtr nsWindow, long visibility)
        {
            var selector = sel_registerName("setTitleVisibility:");
            objc_msgSend_void_long(nsWindow, selector, visibility);
        }

        private static void SetTitlebarAppearsTransparent(IntPtr nsWindow, bool transparent)
        {
            var selector = sel_registerName("setTitlebarAppearsTransparent:");
            objc_msgSend_void_bool(nsWindow, selector, transparent);
        }
    }
}
#endif
