#if __UNO_SKIA_X11__
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Windows.Foundation;

namespace SecureFolderFS.Uno.Platforms.Desktop.Helpers
{
    /// <summary>
    /// Provides helper methods for controlling X11 windows on Linux, covering window operations
    /// that Uno's X11 backend does not implement (interactive window dragging, resizing, and un-maximizing).
    /// </summary>
    internal static partial class X11WindowHelper
    {
        /// <summary>
        /// The thickness, in logical pixels, of the client-drawn resize borders along the window edges.
        /// </summary>
        private const double RESIZE_BORDER_THICKNESS = 8d;

        // https://specifications.freedesktop.org/wm-spec/wm-spec-1.3.html
        private const nint _NET_WM_STATE_REMOVE = 0;
        private const nint _NET_WM_MOVERESIZE_SIZE_TOPLEFT = 0;
        private const nint _NET_WM_MOVERESIZE_SIZE_TOP = 1;
        private const nint _NET_WM_MOVERESIZE_SIZE_TOPRIGHT = 2;
        private const nint _NET_WM_MOVERESIZE_SIZE_RIGHT = 3;
        private const nint _NET_WM_MOVERESIZE_SIZE_BOTTOMRIGHT = 4;
        private const nint _NET_WM_MOVERESIZE_SIZE_BOTTOM = 5;
        private const nint _NET_WM_MOVERESIZE_SIZE_BOTTOMLEFT = 6;
        private const nint _NET_WM_MOVERESIZE_SIZE_LEFT = 7;
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
            return TryBeginMoveResize(window, _NET_WM_MOVERESIZE_MOVE);
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

        /// <summary>
        /// Enables client-drawn resize borders along the window edges. Extending content into the title bar
        /// removes the window manager's decorations, including its resize frame, so edge resizing
        /// has to be reimplemented by the application.
        /// </summary>
        /// <param name="window">The window to enable the resize borders for.</param>
        public static void EnableResizeBorders(Window window)
        {
            if (!OperatingSystem.IsLinux())
                return;

            if (window.Content is not FrameworkElement root)
                return;

            _ = new ResizeBorderTracker(window, root);
        }

        /// <summary>
        /// Determines whether the specified window-space position falls within the client-drawn resize borders.
        /// </summary>
        /// <param name="window">The window to check.</param>
        /// <param name="position">The pointer position relative to the window content.</param>
        /// <returns>True if the position lies on a resize border; otherwise, false.</returns>
        public static bool IsInResizeBorder(Window window, Point position)
        {
            if (window.Content is not FrameworkElement root)
                return false;

            if (!IsResizable(window))
                return false;

            return GetResizeEdge(position, root.ActualWidth, root.ActualHeight) is not null;
        }

        private static bool IsResizable(Window window)
        {
            return window.AppWindow?.Presenter is OverlappedPresenter { IsResizable: true, State: OverlappedPresenterState.Restored };
        }

        private static (nint Action, InputSystemCursorShape Shape)? GetResizeEdge(Point position, double width, double height)
        {
            if (width <= 0d || height <= 0d)
                return null;

            var left = position.X <= RESIZE_BORDER_THICKNESS;
            var right = position.X >= width - RESIZE_BORDER_THICKNESS;
            var top = position.Y <= RESIZE_BORDER_THICKNESS;
            var bottom = position.Y >= height - RESIZE_BORDER_THICKNESS;

            return (top, bottom, left, right) switch
            {
                (true, _, true, _) => (_NET_WM_MOVERESIZE_SIZE_TOPLEFT, InputSystemCursorShape.SizeNorthwestSoutheast),
                (true, _, _, true) => (_NET_WM_MOVERESIZE_SIZE_TOPRIGHT, InputSystemCursorShape.SizeNortheastSouthwest),
                (_, true, true, _) => (_NET_WM_MOVERESIZE_SIZE_BOTTOMLEFT, InputSystemCursorShape.SizeNortheastSouthwest),
                (_, true, _, true) => (_NET_WM_MOVERESIZE_SIZE_BOTTOMRIGHT, InputSystemCursorShape.SizeNorthwestSoutheast),
                (true, _, _, _) => (_NET_WM_MOVERESIZE_SIZE_TOP, InputSystemCursorShape.SizeNorthSouth),
                (_, true, _, _) => (_NET_WM_MOVERESIZE_SIZE_BOTTOM, InputSystemCursorShape.SizeNorthSouth),
                (_, _, true, _) => (_NET_WM_MOVERESIZE_SIZE_LEFT, InputSystemCursorShape.SizeWestEast),
                (_, _, _, true) => (_NET_WM_MOVERESIZE_SIZE_RIGHT, InputSystemCursorShape.SizeWestEast),
                _ => null
            };
        }

        private static bool TryBeginMoveResize(Window window, nint action)
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
                    // grabbed by the button press that initiated the operation, so release the grab first
                    _ = XUngrabPointer(display, IntPtr.Zero);

                    SendClientMessageToRoot(
                        display,
                        x11Window,
                        XInternAtom(display, "_NET_WM_MOVERESIZE", 0),
                        rootX,
                        rootY,
                        action,
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

        /// <summary>
        /// Tracks pointer movement along the window edges to provide resize cursors
        /// and initiate window manager-driven resizing.
        /// </summary>
        private sealed class ResizeBorderTracker
        {
            private static readonly Dictionary<InputSystemCursorShape, InputSystemCursor> _cursorCache = new();
            private static PropertyInfo? _protectedCursorProperty;

            private readonly Window _window;
            private readonly FrameworkElement _root;
            private InputSystemCursorShape? _currentShape;

            public ResizeBorderTracker(Window window, FrameworkElement root)
            {
                _window = window;
                _root = root;

                // Handled events are observed as well, so the borders also work above interactive content
                root.AddHandler(UIElement.PointerMovedEvent, new PointerEventHandler(Root_PointerMoved), true);
                root.AddHandler(UIElement.PointerPressedEvent, new PointerEventHandler(Root_PointerPressed), true);
                root.AddHandler(UIElement.PointerExitedEvent, new PointerEventHandler(Root_PointerExited), true);
            }

            private void Root_PointerMoved(object sender, PointerRoutedEventArgs e)
            {
                var edge = IsResizable(_window)
                    ? GetResizeEdge(e.GetCurrentPoint(_root).Position, _root.ActualWidth, _root.ActualHeight)
                    : null;

                SetCursor(edge?.Shape);
            }

            private void Root_PointerPressed(object sender, PointerRoutedEventArgs e)
            {
                // Do not interfere with elements that already handled the press (e.g. buttons, title bar drag)
                if (e.Handled)
                    return;

                if (!IsResizable(_window))
                    return;

                var point = e.GetCurrentPoint(_root);
                if (!point.Properties.IsLeftButtonPressed)
                    return;

                if (GetResizeEdge(point.Position, _root.ActualWidth, _root.ActualHeight) is not { } edge)
                    return;

                if (TryBeginMoveResize(_window, edge.Action))
                    e.Handled = true;
            }

            private void Root_PointerExited(object sender, PointerRoutedEventArgs e)
            {
                SetCursor(null);
            }

            private void SetCursor(InputSystemCursorShape? shape)
            {
                if (shape == _currentShape)
                    return;

                _currentShape = shape;

                // UIElement.ProtectedCursor is not publicly settable, so reflection is required
                _protectedCursorProperty ??= typeof(UIElement).GetProperty("ProtectedCursor", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (_protectedCursorProperty is null)
                    return;

                InputSystemCursor? cursor = null;
                if (shape is { } cursorShape)
                {
                    if (!_cursorCache.TryGetValue(cursorShape, out cursor))
                    {
                        cursor = InputSystemCursor.Create(cursorShape);
                        _cursorCache[cursorShape] = cursor;
                    }
                }

                try
                {
                    _protectedCursorProperty.SetValue(_root, cursor);
                }
                catch (Exception)
                {
                }
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
