using SecureFolderFS.Core.Sdk.Tracking;
using SecureFolderFS.Core.Paths.DirectoryMetadata.IO;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers
{
    internal abstract class BaseDirectoryIdReceiver : IDirectoryIdReceiver
    {
        protected readonly IDirectoryIdReader directoryIdReader;
        protected readonly IFileSystemStatsTracker? fileSystemStatsTracker;

        protected BaseDirectoryIdReceiver(IDirectoryIdReader directoryIdReader, IFileSystemStatsTracker? fileSystemStatsTracker)
        {
            this.directoryIdReader = directoryIdReader;
            this.fileSystemStatsTracker = fileSystemStatsTracker;
        }

        public virtual DirectoryId GetDirectoryId(string ciphertextPath)
        {
            fileSystemStatsTracker?.AddDirectoryIdAccess();
            return directoryIdReader.ReadDirectoryId(ciphertextPath);
        }

        public DirectoryId CreateNewDirectoryId()
        {
            fileSystemStatsTracker?.AddDirectoryIdAccess();
            return DirectoryId.CreateNew();
        }

        public abstract void RemoveDirectoryId(string ciphertextPath);
    }
}
