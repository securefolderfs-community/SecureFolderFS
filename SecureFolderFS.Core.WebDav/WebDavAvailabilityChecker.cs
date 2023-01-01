using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IFileSystemAvailabilityChecker"/>
    public sealed class WebDavAvailabilityChecker : IFileSystemAvailabilityChecker
    {
        /// <inheritdoc/>
        public Task<FileSystemAvailabilityType> DetermineAvailabilityAsync()
        {
            return Task.FromResult(FileSystemAvailabilityType.Available);
        }
    }
}
