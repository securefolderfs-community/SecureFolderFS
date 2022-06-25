using System.Runtime.InteropServices;
using static SecureFolderFS.WinUI.UnsafeNative.UnsafeNativeDataModels;

namespace SecureFolderFS.WinUI.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport("CoreMessaging.dll")]
        [return: MarshalAs(UnmanagedType.Error)]
        public static extern int CreateDispatcherQueueController(
            [In] DispatcherQueueOptions options,
            [In, Out][MarshalAs(UnmanagedType.IUnknown)] ref object dispatcherQueueController);
    }
}
