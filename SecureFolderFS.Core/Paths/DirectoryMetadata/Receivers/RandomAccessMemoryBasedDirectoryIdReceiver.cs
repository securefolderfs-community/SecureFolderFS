using SecureFolderFS.Core.Paths.DirectoryMetadata.IO;
using SecureFolderFS.Core.Sdk.Tracking;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers
{
    internal sealed class RandomAccessMemoryBasedDirectoryIdReceiver : BaseDirectoryIdReceiver
    {
        private readonly Dictionary<string, DirectoryId> _directoryIds;

        public RandomAccessMemoryBasedDirectoryIdReceiver(IDirectoryIdReader directoryIdReader, IFileSystemStatsTracker? fileSystemStatsTracker)
            : base(directoryIdReader, fileSystemStatsTracker)
        {
            _directoryIds = new(Constants.IO.MAX_CACHED_DIRECTORY_IDS);
        }

        public override DirectoryId GetDirectoryId(string ciphertextPath)
        {
            if (!_directoryIds.TryGetValue(ciphertextPath, out var directoryId))
            {
                fileSystemStatsTracker?.AddDirectoryIdCacheMiss();
                directoryId = base.GetDirectoryId(ciphertextPath);
                AddDirectoryId(ciphertextPath, directoryId);
            }
            else
            {
                fileSystemStatsTracker?.AddDirectoryIdAccess();
                fileSystemStatsTracker?.AddDirectoryIdCacheHit();
            }

            return directoryId;
        }

        public override void RemoveDirectoryId(string ciphertextPath)
        {
            _directoryIds.Remove(ciphertextPath);
        }

        private void AddDirectoryId(string ciphertextPath, DirectoryId directoryId)
        {
            if (_directoryIds.Count >= Constants.IO.MAX_CACHED_DIRECTORY_IDS)
            {
                var directoryIdToRemove = _directoryIds.Keys.First();
                _directoryIds.Remove(directoryIdToRemove);
            }

            _directoryIds[ciphertextPath] = directoryId;
        }
    }
}
