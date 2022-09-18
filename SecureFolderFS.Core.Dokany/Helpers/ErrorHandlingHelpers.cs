using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using SecureFolderFS.Core.Dokany.UnsafeNative;

namespace SecureFolderFS.Core.Dokany.Helpers
{
    internal static class ErrorHandlingHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsFileAlreadyExistsException(IOException ioEx)
        {
            return ioEx.HResult == -2147024816 || ioEx.HResult == -2147024713;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDirectoryNotEmptyException(IOException ioEx)
        {
            return ioEx.HResult == -2147024751;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDiskFullException(IOException ioEx)
        {
            return (uint)ioEx.HResult == 0x80070027 || (uint)ioEx.HResult == 0x80070070;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsSharingViolationException(IOException ioEx)
        {
            return (uint)Marshal.GetHRForException(ioEx) == 0x80070020;
        }

        public static long MakeHResult(uint severity, uint facility, uint code)
        {
            var result = severity << 31;
            result |= facility << 16;
            result |= code;

            return result;
        }

        public static bool Win32FromHResult(long hr, out uint win32Error)
        {
            const int S_OK = 0x00000000;
            const int SEVERITY_ERROR = 1;
            const int FACILITY_WIN32 = 7;

            if (hr == S_OK || (hr & 0xFFFF0000) == (uint)MakeHResult(SEVERITY_ERROR, FACILITY_WIN32, 0u))
            {
                win32Error = ((uint)hr & 0xFFFF);
                return true;
            }

            // Impossible value
            win32Error = 0u;
            return false;
        }

        public static bool Win32ErrorFromException(Exception ex, out uint win32error)
        {
            if (!HResultFromException(ex, out var hr))
            {
                win32error = 0u;
                return false;
            }

            return Win32FromHResult(hr, out win32error);
        }

        public static bool HResultFromException(Exception ex, out long hr)
        {
            hr = ex.HResult;
            if (hr == 0)
                hr = Marshal.GetHRForException(ex);

            return hr != 0;
        }

        public static bool NtStatusFromException(Exception ex, out long ntStatus)
        {
            if (!Win32ErrorFromException(ex, out var win32error))
            {
                ntStatus = -1L;
                return false;
            }

            ntStatus = UnsafeNativeApis.DokanNtStatusFromWin32(win32error);
            return true;
        }
    }
}
