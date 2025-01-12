// Some parts of the following code were used from WinUIEx on the MIT License basis.
// See the associated license file for more information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.UI.Xaml;
using SecureFolderFS.Uno.PInvoke;
using SecureFolderFS.Uno.Platforms.Windows.Extensions;
using Vanara.PInvoke;
using Windows.Storage;
using static Vanara.PInvoke.User32;

namespace SecureFolderFS.Uno.Platforms.Windows.Helpers
{
    internal sealed class WindowsBoundsManager : IDisposable
    {
        private static readonly Dictionary<Window, WindowsBoundsManager> _instances = new();
        private readonly Win32NativeWindowMessageReceiver _messageReceiver;
        private readonly Window _window;
        private readonly IntPtr _hWnd;
        private bool _isRestoringWindowState;

        public int MinWidth { get; set; }

        public int MinHeight { get; set; }

        private WindowsBoundsManager(Window window)
        {
            _window = window;
            _hWnd = WinRT.Interop.WindowNative.GetWindowHandle(window);
            _messageReceiver = new(_hWnd);
            _messageReceiver.MessageReceived += MessageReceiver_MessageReceived;
        }

        public bool LoadWindowState(string windowId)
        {
#if UNPACKAGED
            // Unpackaged scenario is currently unsupported
            return false;
#endif
            var dataContainer = GetDataContainer(false);
            if (dataContainer is null)
                return false;

            var dataKey = $"WindowState_{windowId}";
            byte[]? windowStateBuffer = null;

            if (dataContainer.TryGetValue(dataKey, out var rawObj) && rawObj is string base64)
                windowStateBuffer = Convert.FromBase64String(base64);

            if (windowStateBuffer is null)
                return false;

            var monitors = UnsafeNative.GetMonitorInfos();
            using var memoryStream = new MemoryStream(windowStateBuffer);
            using var binaryReader = new BinaryReader(memoryStream);

            // Read monitors count
            var monitorCount = binaryReader.ReadInt32();
            if (monitorCount != monitors.Count)
                return false; // Amount of monitors changed

            foreach (var item in monitors)
            {
                // Skip monitor name (important, do not remove)
                _ = binaryReader.ReadString();

                var left = binaryReader.ReadInt32();
                var top = binaryReader.ReadInt32();
                var right = binaryReader.ReadInt32();
                var bottom = binaryReader.ReadInt32();

                if (item.rcMonitor.Left != left ||
                    item.rcMonitor.Top != top ||
                    item.rcMonitor.Right != right ||
                    item.rcMonitor.Bottom != bottom)
                    return false; // Monitor layout changed
            }

            var sizeWindowPlacement = Marshal.SizeOf<WINDOWPLACEMENT>();
            var windowPlacementBuffer = binaryReader.ReadBytes(sizeWindowPlacement);
            var pWindowPlacementBuffer = Marshal.AllocHGlobal(sizeWindowPlacement);

            Marshal.Copy(windowPlacementBuffer, 0, pWindowPlacementBuffer, sizeWindowPlacement);
            var windowPlacement = Marshal.PtrToStructure<WINDOWPLACEMENT>(pWindowPlacementBuffer);
            Marshal.FreeHGlobal(pWindowPlacementBuffer);

            if (windowPlacement is { showCmd: ShowWindowCommand.SW_SHOWMINIMIZED, flags: WindowPlacementFlags.WPF_RESTORETOMAXIMIZED })
                windowPlacement.showCmd = ShowWindowCommand.SW_MAXIMIZE;
            else if (windowPlacement.showCmd != ShowWindowCommand.SW_MAXIMIZE)
                windowPlacement.showCmd = ShowWindowCommand.SW_NORMAL;

            _isRestoringWindowState = true;
            _ = SetWindowPlacement(_window.GetWindowHandle(), ref windowPlacement);
            _isRestoringWindowState = false;

            return true;
        }

        public void SaveWindowState(string windowId)
        {
            var dataContainer = GetDataContainer(true);
            if (dataContainer is null)
                return;

            using var memoryStream = new MemoryStream();
            using var binaryWriter = new BinaryWriter(memoryStream);
            var monitors = UnsafeNative.GetMonitorInfos();

            binaryWriter.Write(monitors.Count);
            foreach (var item in monitors)
            {
                binaryWriter.Write(item.szDevice);
                binaryWriter.Write(item.rcMonitor.Left);
                binaryWriter.Write(item.rcMonitor.Top);
                binaryWriter.Write(item.rcMonitor.Right);
                binaryWriter.Write(item.rcMonitor.Bottom);
            }

            var windowPlacement = new WINDOWPLACEMENT();
            _ = GetWindowPlacement(_window.GetWindowHandle(), ref windowPlacement);

            var sizeWindowPlacement = Marshal.SizeOf<WINDOWPLACEMENT>();
            var pWindowPlacementBuffer = Marshal.AllocHGlobal(sizeWindowPlacement);
            Marshal.StructureToPtr(windowPlacement, pWindowPlacementBuffer, false);
            
            var windowPlacementBuffer = new byte[sizeWindowPlacement];
            Marshal.Copy(pWindowPlacementBuffer, windowPlacementBuffer, 0, sizeWindowPlacement);
            Marshal.FreeHGlobal(pWindowPlacementBuffer);

            binaryWriter.Write(windowPlacementBuffer);
            binaryWriter.Flush();
            dataContainer[$"WindowState_{windowId}"] = Convert.ToBase64String(memoryStream.ToArray());
        }

        private IDictionary<string, object>? GetDataContainer(bool createIfMissing)
        {
            try
            {
                if (ApplicationData.Current.LocalSettings.Containers.TryGetValue(UI.Constants.DATA_CONTAINER_ID, out var container))
                    return container.Values;
                else if (createIfMissing)
                    return ApplicationData.Current.LocalSettings.CreateContainer(UI.Constants.DATA_CONTAINER_ID, ApplicationDataCreateDisposition.Always).Values;
            }
            catch (Exception) { }

            return null;
        }

        private void MessageReceiver_MessageReceived(object? sender, WindowMessageEventArgs e)
        {
            switch ((WindowMessage)e.NativeMessage.uMsg)
            {
                case WindowMessage.WM_GETMINMAXINFO:
                unsafe
                {
                    if (_isRestoringWindowState)
                        break;

                    var rect2 = (MINMAXINFO*)e.NativeMessage.lparam;
                    var currentDpi = GetDpiForWindow(_hWnd);

                    // Restrict min-size
                    rect2->minTrackSize.cx = (int)Math.Max(MinWidth * (currentDpi / 96f), rect2->minTrackSize.cx);
                    rect2->minTrackSize.cy = (int)Math.Max(MinHeight * (currentDpi / 96f), rect2->minTrackSize.cy);

                    break;
                }

                case WindowMessage.WM_DPICHANGED:
                {
                    if (_isRestoringWindowState)
                        e.Handled = true;

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
