using SecureFolderFS.Core.Paths.DirectoryMetadata;
using SecureFolderFS.Core.Security;
using SecureFolderFS.Core.Tracking;

namespace SecureFolderFS.Core.FileNames.Receivers
{
    internal sealed class InstantAccessBasedFileNameReceiver : BaseFileNameReceiver
    {
        public InstantAccessBasedFileNameReceiver(ISecurity security, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(security, fileSystemStatsTracker)
        {
        }

        public override void SetCiphertextFileName(DirectoryId directoryId, string cleartextFileName, string ciphertextFileName)
        {
        }

        public override void SetCleartextFileName(DirectoryId directoryId, string ciphertextFileName, string cleartextFileName)
        {
        }
    }
}
