using System.Linq;
using System.Collections.Generic;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Core.Tracking;
using SecureFolderFS.Core.Paths.DirectoryMetadata.IO;

namespace SecureFolderFS.Core.Paths.DirectoryMetadata.Receivers
{
    internal sealed class RandomAccessMemoryBasedDirectoryIdReceiver : BaseDirectoryIdReceiver
    {
        private readonly Dictionary<string, DirectoryId> _directoryIds;

        public RandomAccessMemoryBasedDirectoryIdReceiver(IDirectoryIdReader directoryIdReader, IFileSystemStatsTracker fileSystemStatsTracker)
            : base(directoryIdReader, fileSystemStatsTracker)
        {
            this._directoryIds = new Dictionary<string, DirectoryId>();
        }

        public override DirectoryId GetDirectoryId(string ciphertextPath)
        {
            AssertNotDisposed();

            if (!_directoryIds.TryGetValue(ciphertextPath, out DirectoryId directoryId))
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

            _directoryIds.AddOrReplace(ciphertextPath, directoryId);
        }

        public override void Dispose()
        {
            _directoryIds.Clear();

            base.Dispose();
        }
    }
}
