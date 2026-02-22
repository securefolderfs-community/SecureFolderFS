using System;
using System.Reflection;
using Microsoft.UI.Xaml;
using Uno.UI.Xaml;
using static SecureFolderFS.Uno.PInvoke.UnsafeNative;

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    /// <summary>
    /// Provides helper methods for configuring macOS title bar to extend content into the title bar area
    /// while preserving the native window chrome (traffic light buttons and rounded corners).
    /// </summary>
    internal static class MacOsTitleBarHelper
    {
        // NSWindowStyleMask values (for reference)
        private const ulong NSWindowStyleMaskFullSizeContentView = 1 << 15;

        // NSWindowTitleVisibility values
        private const long NSWindowTitleHidden = 1;
        
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
        
        private static IntPtr GetNativeWindowHandle(Window window)
        {
#if __UNO_SKIA_MACOS__
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
#else
            return IntPtr.Zero;
#endif
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

