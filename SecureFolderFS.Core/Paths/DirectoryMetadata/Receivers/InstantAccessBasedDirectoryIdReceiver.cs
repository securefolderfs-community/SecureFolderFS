using SecureFolderFS.Core.Paths.DirectoryMetadata.IO;
using SecureFolderFS.Core.Tracking;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers
{
    internal sealed class InstantAccessBasedDirectoryIdReceiver : BaseDirectoryIdReceiver, IDirectoryIdReceiver
    {
        public InstantAccessBasedDirectoryIdReceiver(IDirectoryIdReader directoryIdReader, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(directoryIdReader, fileSystemStatsTracker)
        {
        }

        public override void RemoveDirectoryId(string ciphertextPath)
        {
        }
    }
}
