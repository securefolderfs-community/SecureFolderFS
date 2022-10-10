using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IFileSystemDescriptor"/>
    public sealed class DokanyDescriptor : IFileSystemDescriptor
    {
        /// <inheritdoc/>
        public Task<FileSystemAvailabilityType> DetermineAvailabilityAsync()
        {
            ulong dokanVersion;
            ulong dokanDriverVersion;

            try
            {
                dokanVersion = UnsafeNativeApis.DokanVersion();
                if (dokanVersion <= 0)
                    return Task.FromResult(FileSystemAvailabilityType.ModuleNotAvailable);
            }
            catch (Exception)
            {
                return Task.FromResult(FileSystemAvailabilityType.ModuleNotAvailable);
            }

            try
            {
                dokanDriverVersion = UnsafeNativeApis.DokanDriverVersion();
                if (dokanDriverVersion <= 0)
                    return Task.FromResult(FileSystemAvailabilityType.CoreNotAvailable);
            }
            catch (Exception)
            {
                return Task.FromResult(FileSystemAvailabilityType.CoreNotAvailable);
            }

            var error = FileSystemAvailabilityType.None;
            error |= dokanVersion > Constants.DOKAN_MAX_VERSION ? FileSystemAvailabilityType.VersionTooHigh : error;
            error |= dokanVersion < Constants.DOKAN_VERSION ? FileSystemAvailabilityType.VersionTooLow : error;

            error = error == FileSystemAvailabilityType.None ? FileSystemAvailabilityType.Available : error;
            return Task.FromResult(error);
        }
    }
}
