using System.Runtime.InteropServices;
using SecureFolderFS.WinUI.UnsafeNative;
using static SecureFolderFS.WinUI.UnsafeNative.UnsafeNativeDataModels;

namespace SecureFolderFS.WinUI.Helpers
{
    internal sealed class WindowsSystemDispatcherQueueHelper
    {
        private object? _dispatcherQueueController;

        public void EnsureWindowsSystemDispatcherQueueController()
        {
            const int DQTYPE_THREAD_CURRENT = 2;
            const int DQTAT_COM_STA = 2;

            if (Windows.System.DispatcherQueue.GetForCurrentThread() is not null)
            {
                // Already exists, so we'll just use it.
                return;
            }

            if (_dispatcherQueueController is null)
            {
                DispatcherQueueOptions options = new();

                options.dwSize = (uint)Marshal.SizeOf(typeof(DispatcherQueueOptions));
                options.threadType = DQTYPE_THREAD_CURRENT;
                options.apartmentType = DQTAT_COM_STA;

                UnsafeNativeApis.CreateDispatcherQueueController(options, ref _dispatcherQueueController!);
            }
        }
    }
}
