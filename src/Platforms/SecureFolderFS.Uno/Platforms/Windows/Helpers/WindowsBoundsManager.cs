using System;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Vanara.PInvoke;

namespace SecureFolderFS.Uno.Platforms.Windows.Helpers
{
    internal sealed class WindowsBoundsManager : IDisposable
    {
        private static readonly Dictionary<Window, WindowsBoundsManager> _instances = new();
        private readonly Win32NativeWindowMessageReceiver _messageReceiver;
        private readonly Window _window;
        private readonly IntPtr _hWnd;

        public int MinWidth { get; set; }

        public int MinHeight { get; set; }

        private WindowsBoundsManager(Window window)
        {
            _window = window;
            _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            _messageReceiver = new(_hWnd);
            _messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
        }

        private void MessageReceiver_MessageReceived(object? sender, WindowMessageEventArgs e)
        {
            switch ((User32.WindowMessage)e.NativeMessage.uMsg)
            {
                case User32.WindowMessage.WM_GETMINMAXINFO:
                unsafe
                {
                    var rect2 = (User32.MINMAXINFO*)e.NativeMessage.lparam;
                    var currentDpi = User32.GetDpiForWindow(_hWnd);

                    // Restrict min-size
                    rect2->minTrackSize.cx = (int)Math.Max(MinWidth * (currentDpi / 96f), rect2->minTrackSize.cx);
                    rect2->minTrackSize.cy = (int)Math.Max(MinHeight * (currentDpi / 96f), rect2->minTrackSize.cy);
                    break;
                }
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _messageReceiver.MessageReceived -= MessageReceiver_MessageReceived;
            _messageReceiver.Dispose();
            _instances.Remove(_window);
        }

        public static WindowsBoundsManager AddOrGet(Window window)
        {
            if (_instances.TryGetValue(window, out var boundsManager))
                return boundsManager;

            boundsManager = new(window);
            _instances.Add(window, boundsManager);

            return boundsManager;
        }
    }

    internal sealed class Win32NativeWindowMessageReceiver : IDisposable
    {
        private readonly IntPtr _hWnd;
        private ComCtl32.SUBCLASSPROC? _subclassprocCallback;

        private event EventHandler<WindowMessageEventArgs>? _messageReceived;
        public event EventHandler<WindowMessageEventArgs>? MessageReceived
        {
            add
            {
                if (_messageReceived is null)
                    NativeSubscribe();

                _messageReceived += value;
            }
            remove
            {
                _messageReceived -= value;
                if (_messageReceived is null)
                    NativeUnsubscribe();
            }
        }

        public Win32NativeWindowMessageReceiver(IntPtr hWnd)
        {
            _hWnd = hWnd;
        }

        private IntPtr WindowProc(HWND hWnd, uint uMsg, IntPtr wParam, IntPtr lParam, UIntPtr uIdSubclass, IntPtr dwRefData)
        {
            var args = new WindowMessageEventArgs(new(_hWnd, lParam, wParam, uMsg));
            _messageReceived?.Invoke(this, args);
            if (args.Handled)
                return args.Result;

            return ComCtl32.DefSubclassProc(hWnd, uMsg, wParam, lParam);
        }

        private void NativeSubscribe()
        {
            if (_subclassprocCallback is not null)
                return;

            _subclassprocCallback = new(WindowProc);
            var result = ComCtl32.SetWindowSubclass(_hWnd, _subclassprocCallback, 101, 0);
        }

        private void NativeUnsubscribe()
        {
            if (_subclassprocCallback is null)
                return;

            ComCtl32.RemoveWindowSubclass(_hWnd, _subclassprocCallback, 101);
            _subclassprocCallback = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            NativeUnsubscribe();
            _messageReceived = null;
        }
    }

    internal sealed class WindowMessageEventArgs(Win32Message nativeMessage) : EventArgs
    {
        public Win32Message NativeMessage { get; } = nativeMessage;

        public IntPtr Result { get; set; }

        public bool Handled { get; set; }
    }

    internal readonly struct Win32Message(IntPtr hwnd, IntPtr lparam, IntPtr wparam, uint uMsg)
    {
        public readonly IntPtr hwnd = hwnd;
        public readonly IntPtr lparam = lparam;
        public readonly IntPtr wparam = wparam;
        public readonly uint uMsg = uMsg;
    }
}
