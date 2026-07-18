using FuseSharp;
using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MacFuse
{
    /// <inheritdoc cref="IFileSystemInfo"/>
    public sealed partial class MacFuseFileSystem
    {
        /// <inheritdoc/>
        public partial async Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            try
            {
                return Fuse.CheckDependencies() ? FileSystemAvailability.Available : FileSystemAvailability.ModuleUnavailable;
            }
            catch (TypeInitializationException)
            {
                return FileSystemAvailability.ModuleUnavailable;
            }
        }
    }
}
