using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Directories;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public sealed class InstantDirectoryIdAccess : BaseDirectoryIdAccess
    {
        public InstantDirectoryIdAccess(IDirectoryIdStreamAccess directoryIdStreamAccess, IFileSystemStatistics? fileSystemStatistics, IFileSystemHealthStatistics? fileSystemHealthStatistics)
            : base(directoryIdStreamAccess, fileSystemStatistics, fileSystemHealthStatistics)
        {
        }
    }
}
