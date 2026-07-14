#if __UNO_SKIA_X11__
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    /// <summary>
    /// Provides helper methods for controlling X11 windows on Linux, covering window operations
    /// that Uno's X11 backend does not implement (interactive window dragging and un-maximizing).
    /// </summary>
    internal static partial class X11WindowHelper
    {
        // https://specifications.freedesktop.org/wm-spec/wm-spec-1.3.html
        private const nint _NET_WM_STATE_REMOVE = 0;
        private const nint _NET_WM_MOVERESIZE_MOVE = 8;
        private const nint SOURCE_INDICATION_APPLICATION = 1;
        private const long SubstructureNotifyMask = 1L << 19;
        private const long SubstructureRedirectMask = 1L << 20;
        private const int ClientMessage = 33;

        private static MethodInfo? _getHostFromWindowMethod;
        private static PropertyInfo? _rootX11WindowProperty;

        /// <summary>
        /// Begins an interactive, window manager-driven move of the window, equivalent to dragging its title bar.
        /// Call while a pointer button is pressed; the window manager takes over the drag from there.
        /// </summary>
        /// <param name="window">The window to move.</param>
        /// <returns>True if the move request was sent to the window manager; otherwise, false.</returns>
        public static bool TryBeginWindowDrag(Window window)
        {
            if (!OperatingSystem.IsLinux())
                return false;

            try
            {
                if (GetX11Handles(window) is not { } handles)
                    return false;

                var (display, x11Window) = handles;
                XLockDisplay(display);
                try
                {
                    // Locate the pointer in root-window coordinates
                    var rootWindow = XDefaultRootWindow(display);
                    if (XQueryPointer(display, rootWindow, out _, out _, out var rootX, out var rootY, out _, out _, out _) == 0)
                        return false;

                    // The window manager cannot take over the pointer while it is still implicitly
                    // grabbed by the button press that initiated the drag, so release the grab first
                    _ = XUngrabPointer(display, IntPtr.Zero);

                    SendClientMessageToRoot(
                        display,
                        x11Window,
                        XInternAtom(display, "_NET_WM_MOVERESIZE", 0),
                        rootX,
                        rootY,
                        _NET_WM_MOVERESIZE_MOVE,
                        1 /* left mouse button */,
                        SOURCE_INDICATION_APPLICATION);

                    return true;
                }
                finally
                {
                    XUnlockDisplay(display);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Restores the window from its maximized state back to a floating window.
        /// Works around Uno's X11 <c>OverlappedPresenter.Restore</c>, which only un-minimizes
        /// and never clears the window manager's maximized state.
        /// </summary>
        /// <param name="window">The window to un-maximize.</param>
        /// <returns>True if the request was sent to the window manager; otherwise, false.</returns>
        public static bool TryUnmaximizeWindow(Window window)
        {
            if (!OperatingSystem.IsLinux())
                return false;

            try
            {
                if (GetX11Handles(window) is not { } handles)
                    return false;

                var (display, x11Window) = handles;
                XLockDisplay(display);
                try
                {
                    SendClientMessageToRoot(
                        display,
                        x11Window,
                        XInternAtom(display, "_NET_WM_STATE", 0),
                        _NET_WM_STATE_REMOVE,
                        XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_HORZ", 0),
                        XInternAtom(display, "_NET_WM_STATE_MAXIMIZED_VERT", 0),
                        SOURCE_INDICATION_APPLICATION,
                        0);

                    return true;
                }
                finally
                {
                    XUnlockDisplay(display);
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void SendClientMessageToRoot(IntPtr display, IntPtr x11Window, IntPtr messageType, nint data0, nint data1, nint data2, nint data3, nint data4)
        {
            var xEvent = new XClientMessageEvent()
            {
                type = ClientMessage,
                send_event = 1,
                window = x11Window,
                message_type = messageType,
                format = 32,
                data0 = data0,
                data1 = data1,
                data2 = data2,
                data3 = data3,
                data4 = data4
            };

            _ = XSendEvent(display, XDefaultRootWindow(display), 0, (IntPtr)(SubstructureRedirectMask | SubstructureNotifyMask), ref xEvent);
            _ = XFlush(display);
        }

        private static (IntPtr Display, IntPtr Window)? GetX11Handles(Window window)
        {
            try
            {
                // Uno does not expose the X11 Display handle publicly, so it has to be
                // retrieved from the internal X11XamlRootHost associated with the window
                _getHostFromWindowMethod ??= Type
                    .GetType("Uno.WinUI.Runtime.Skia.X11.X11XamlRootHost, Uno.UI.Runtime.Skia.X11")?
                    .GetMethod("GetHostFromWindow", BindingFlags.Public | BindingFlags.Static);

                var host = _getHostFromWindowMethod?.Invoke(null, [ window ]);
                if (host is null)
                    return null;

                _rootX11WindowProperty ??= host.GetType().GetProperty("RootX11Window", BindingFlags.Public | BindingFlags.Instance);
                var rootX11Window = _rootX11WindowProperty?.GetValue(host);
                if (rootX11Window is null)
                    return null;

                var x11WindowType = rootX11Window.GetType();
                var display = x11WindowType.GetProperty("Display")?.GetValue(rootX11Window);
                var x11Window = x11WindowType.GetProperty("Window")?.GetValue(rootX11Window);
                if (display is not IntPtr displayHandle || x11Window is not IntPtr windowHandle)
                    return null;

                if (displayHandle == IntPtr.Zero || windowHandle == IntPtr.Zero)
                    return null;

                return (displayHandle, windowHandle);
            }
            catch (Exception)
            {
                return null;
            }
        }

        // Mirrors XClientMessageEvent with 32-bit format data (l[5]), padded to the full XEvent union size
        [StructLayout(LayoutKind.Sequential, Size = 192)]
        private struct XClientMessageEvent
        {
            public int type;
            public nuint serial;
            public int send_event;
            public IntPtr display;
            public IntPtr window;
            public IntPtr message_type;
            public int format;
            public nint data0;
            public nint data1;
            public nint data2;
            public nint data3;
            public nint data4;
        }

        [LibraryImport("libX11.so.6", StringMarshalling = StringMarshalling.Utf8)]
        private static partial IntPtr XInternAtom(IntPtr display, string atomName, int onlyIfExists);

        [LibraryImport("libX11.so.6")]
        private static partial IntPtr XDefaultRootWindow(IntPtr display);

        [LibraryImport("libX11.so.6")]
        private static partial int XQueryPointer(IntPtr display, IntPtr window, out IntPtr rootReturn, out IntPtr childReturn, out int rootX, out int rootY, out int winX, out int winY, out uint maskReturn);

        [LibraryImport("libX11.so.6")]
        private static partial int XUngrabPointer(IntPtr display, IntPtr time);

        [LibraryImport("libX11.so.6")]
        private static partial int XSendEvent(IntPtr display, IntPtr window, int propagate, IntPtr eventMask, ref XClientMessageEvent xEvent);

        [LibraryImport("libX11.so.6")]
        private static partial int XFlush(IntPtr display);

        [LibraryImport("libX11.so.6")]
        private static partial void XLockDisplay(IntPtr display);

        [LibraryImport("libX11.so.6")]
        private static partial void XUnlockDisplay(IntPtr display);
    }
}
#endif
