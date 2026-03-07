using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WinFsp
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed partial class WinFspFileSystem
    {
        /// <inheritdoc/>
        public partial Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(IsSupported());
        }

        private static FileSystemAvailability IsSupported()
        {
            try
            {
                Fsp.Service.Log(0, "VersionCheck");
                return FileSystemAvailability.Available;
            }
            catch (Exception)
            {
                return FileSystemAvailability.ModuleUnavailable;
            }
        }
    }
}
