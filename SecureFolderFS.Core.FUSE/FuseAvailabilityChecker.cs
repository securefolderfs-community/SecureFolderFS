using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using Tmds.Fuse;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IFileSystemAvailabilityChecker"/>
    public sealed class FuseAvailabilityChecker : IFileSystemAvailabilityChecker
    {
        /// <inheritdoc/>
        public Task<FileSystemAvailabilityType> DetermineAvailabilityAsync()
        {

            return Task.FromResult(Fuse.CheckDependencies() ? FileSystemAvailabilityType.Available : FileSystemAvailabilityType.ModuleNotAvailable);
        }
    }
}