using System;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.UnsafeNative;

namespace SecureFolderFS.Core.Helpers
{
    /// <summary>
    /// Provides implementation for checking availability of installed file system providers on user machine.
    /// <br/>
    /// <br/>
    /// This SDK is exposed.
    /// </summary>
    public static class FileSystemAvailabilityHelpers
    {
        public static FileSystemAvailabilityErrorType IsDokanyAvailable()
        {
            ulong dokanVersion;
            ulong dokanDriverVersion;

            try
            {
                dokanVersion = UnsafeNativeApis.DokanVersion();
                if (dokanVersion <= 0)
                    return FileSystemAvailabilityErrorType.ModuleNotAvailable;
            }
            catch (Exception)
            {
                return FileSystemAvailabilityErrorType.ModuleNotAvailable;
            }

            try
            {
                dokanDriverVersion = UnsafeNativeApis.DokanDriverVersion();
                if (dokanDriverVersion <= 0)
                    return FileSystemAvailabilityErrorType.DriverNotAvailable;
            }
            catch (Exception)
            {
                return FileSystemAvailabilityErrorType.DriverNotAvailable;
            }

            var error = FileSystemAvailabilityErrorType.None;
            error |= dokanVersion > Constants.FileSystem.Dokan.DOKAN_MAX_VERSION ? FileSystemAvailabilityErrorType.VersionTooHigh : error;
            error |= dokanVersion < Constants.FileSystem.Dokan.DOKAN_VERSION ? FileSystemAvailabilityErrorType.VersionTooLow : error;

            return error == FileSystemAvailabilityErrorType.None ? FileSystemAvailabilityErrorType.FileSystemAvailable : error;
        }

        internal static FileSystemAdapterType GetAvailableAdapter(FileSystemAdapterType defaultValue = FileSystemAdapterType.DokanAdapter)
        {
            if (defaultValue == FileSystemAdapterType.DokanAdapter && IsDokanyAvailable() == FileSystemAvailabilityErrorType.FileSystemAvailable)
            {
                return defaultValue;
            }
            else
            {
                if (IsDokanyAvailable() == FileSystemAvailabilityErrorType.FileSystemAvailable)
                {
                    return FileSystemAdapterType.DokanAdapter;
                }
                // else if () { ... }

                throw new UnavailableFileSystemAdapterException();
            }
        }
    }
}
