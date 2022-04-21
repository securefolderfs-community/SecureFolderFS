using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SecureFolderFS.Core.Helpers
{
    internal static class ErrorHandlingHelpers
    {
        public static int MakeHResult(uint facility, uint errorNo)
        {
            // Make HR
            uint result = 1U << 31;
            result |= facility << 16;
            result |= errorNo;

            return unchecked((int)result);
        }

        public static bool Win32FromHresult(int hresult, out uint win32Error)
        {
            const int S_OK = 0x00000000;
            const int S_ERROR = 1;
            const int FACILITY_WIN32 = 7;

            if ((hresult & 0xFFFF0000) == MakeHResult(FACILITY_WIN32, S_ERROR)
                || hresult == S_OK
                || hresult < 0)
            {
                win32Error = (uint)hresult;
                return true;
            }
            else
            {
                // Impossible value
                win32Error = 0x0u;
                return false;
            }
        }

        public static bool Win32ErrorFromException(this Exception exception, out uint win32error)
        {
            if (!HresultFromException(exception, out var hresult))
            {
                win32error = 0x0u;
                return false;
            }

            return Win32FromHresult(hresult, out win32error);
        }

        public static bool HresultFromException(this Exception exception, out int hresult)
        {
            ArgumentNullException.ThrowIfNull(exception);

            hresult = exception.HResult;

            if (hresult == 0)
            {
                hresult = Marshal.GetHRForException(exception);
            }
            if (hresult == 0)
            {
                return false;
            }

            return true;
        }

        public static bool IsFileAlreadyExistsException(this IOException ioex)
        {
            return ioex.HResult == -2147024816 || ioex.HResult == -2147024713;
        }

        public static bool IsDirectoryNotEmptyException(this IOException ioex)
        {
            return ioex.HResult == -2147024751;
        }

        public static bool IsDiskFullException(this IOException ioex)
        {
            return (uint)ioex.HResult == 0x80070027 || (uint)ioex.HResult == 0x80070070;
        }

        public static ArgumentException GetBadTypeException(string parameterName, Type requestedType)
        {
            return new ArgumentException($"{parameterName} was not of an expected type: {requestedType.Name}");
        }
    }
}
