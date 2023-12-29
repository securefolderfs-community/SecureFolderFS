using System;
using Microsoft.UI.Xaml;

namespace SecureFolderFS.Uno.Windows.Extensions
{
    public static class WindowingExtensions
    {
        public static IntPtr GetWindowHandle(this Window window)
        {
            return WinRT.Interop.WindowNative.GetWindowHandle(window);
        }
    }
}
