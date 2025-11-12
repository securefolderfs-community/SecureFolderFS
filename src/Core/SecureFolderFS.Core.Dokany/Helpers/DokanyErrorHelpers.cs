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

            ntStatus = UnsafeNativeApis.DokanNtStatusFromWin32(win32error);
            return true;
        }
    }
}
