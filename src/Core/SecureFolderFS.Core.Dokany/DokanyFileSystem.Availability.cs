using SecureFolderFS.Core.Dokany.UnsafeNative;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed partial class DokanyFileSystem
    {
        /// <inheritdoc/>
        public partial Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(IsSupported());
        }

        private static FileSystemAvailability IsSupported()
        {
            ulong dokanVersion;
            ulong dokanDriverVersion;

            try
            {
                dokanVersion = UnsafeNativeApis.DokanVersion();
                if (dokanVersion <= 0)
                    return FileSystemAvailability.ModuleUnavailable;
            }
            catch (Exception)
            {
                return FileSystemAvailability.ModuleUnavailable;
            }

            try
            {
                dokanDriverVersion = UnsafeNativeApis.DokanDriverVersion();
                if (dokanDriverVersion <= 0)
                    return FileSystemAvailability.CoreUnavailable;
            }
            catch (Exception)
            {
                return FileSystemAvailability.CoreUnavailable;
            }

            var error = FileSystemAvailability.None;
            error |= dokanVersion > Constants.Dokan.DOKAN_MAX_VERSION ? FileSystemAvailability.VersionTooHigh : error;
            error |= dokanVersion < Constants.Dokan.DOKAN_VERSION ? FileSystemAvailability.VersionTooLow : error;

            return error == FileSystemAvailability.None ? FileSystemAvailability.Available : error;
        }
    }
}
