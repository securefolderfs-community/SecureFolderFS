using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem.Helpers;
using System;

namespace SecureFolderFS.Core.Dokany.Helpers
{
    internal static class DokanyErrorHelpers
    {
        public static bool NtStatusFromException(Exception ex, out long ntStatus)
        {
            if (!ErrorHandlingHelpers.Win32ErrorFromException(ex, out var win32error))
            {
                ntStatus = -1L;
                return false;
            }

            ntStatus = Win32ErrorToNtStatus(win32error);
            return true;
        }

        public static long Win32ErrorToNtStatus(uint win32Error)
        {
            // Convert common Win32 error codes to NT status codes
            return win32Error switch
            {
                0 => 0x00000000,
                2 => unchecked((long)0xC0000034),
                3 => unchecked((long)0xC000003A),
                5 => unchecked((long)0xC0000022),
                32 => unchecked((long)0xC0000043),
                80 => unchecked((long)0xC0000035),
                112 => unchecked((long)0xC000007F),
                183 => unchecked((long)0xC0000035),
                _ => UnsafeNativeApis.DokanNtStatusFromWin32((uint)win32Error)
            };
        }
    }
}
