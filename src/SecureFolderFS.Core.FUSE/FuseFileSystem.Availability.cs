using SecureFolderFS.Storage.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IFileSystem"/>
    public sealed partial class FuseFileSystem
    {
        /// <inheritdoc/>
        public partial async Task<FileSystemAvailability> GetStatusAsync(CancellationToken cancellationToken)
        {
            await Task.CompletedTask;
            try
            {
                return Fuse.CheckDependencies() ? FileSystemAvailability.Available : FileSystemAvailability.ModuleUnavailable;
            }
            catch (TypeInitializationException) // Fuse might sometimes throw (tested on Windows)
            {
                return FileSystemAvailability.ModuleUnavailable;
            }
        }
    }
}
