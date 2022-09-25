using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.UnsafeNative
{
    internal static class UnsafeNativeApis
    {
        [DllImport(Dokany.Constants.DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong DokanVersion();

        [DllImport(Dokany.Constants.DOKAN_DLL, ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.U8)]
        public static extern ulong DokanDriverVersion();
    }
}
