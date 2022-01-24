using SecureFolderFS.Core.Exceptions;
using SecureFolderFS.Core.FileSystem.FileSystemAdapter;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Core.Paths;
using SecureFolderFS.Core.Streams.Receiver;
using SecureFolderFS.Core.Tracking;

namespace SecureFolderFS.Core.Instance.Implementation
{
    internal sealed class SecureFolderFSInstance : ISecureFolderFSInstance
    {
        public string MountLocation { get; internal set; }

        public IPathReceiver PathReceiver { get; internal set; }

        internal IFileSystemStatsTracker FileSystemStatsTracker { get; set; }

        internal IFileStreamReceiver FileStreamReceiver { get; set; }

        internal IFileSystemAdapter FileSystemAdapter { get; set; }

        internal IFileSystemOperations FileSystemOperations { get; set; }

        public void StartFileSystem()
        {
            if (MountLocation == null)
            {
                var mountLetter = VaultHelpers.GetUnusedMountLetter();
                if (mountLetter == char.MinValue)
                {
                    throw new VaultInitializationException("No free mount point exists to mount the vault.");
                }

                MountLocation = $"{mountLetter}:\\";
            }

            FileSystemAdapter.StartFileSystem(MountLocation);
        }

        public void StopFileSystem()
        {
            FileSystemAdapter.StopFileSystem(MountLocation);
        }

        public void Dispose()
        {
            FileSystemAdapter?.StopFileSystem(MountLocation);
            PathReceiver?.Dispose();
            FileSystemStatsTracker?.Dispose();
            FileStreamReceiver?.Dispose();
            FileSystemAdapter?.Dispose();
        }
    }
}
