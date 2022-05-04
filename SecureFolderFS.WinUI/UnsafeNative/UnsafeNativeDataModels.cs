using System.Runtime.InteropServices;

namespace SecureFolderFS.WinUI.UnsafeNative
{
    internal static class UnsafeNativeDataModels
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct DispatcherQueueOptions
        {
            [MarshalAs(UnmanagedType.U4)]
            public uint dwSize;

            public int threadType;

            public int apartmentType;
        }
    }
}
