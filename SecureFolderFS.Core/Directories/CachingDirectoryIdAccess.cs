using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.FileSystem.Analytics;
using SecureFolderFS.Core.FileSystem.Directories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public sealed class CachingDirectoryIdAccess : BaseDirectoryIdAccess
    {
        private readonly Dictionary<string, DirectoryIdBuffer> _directoryIdCache;
        private readonly object _lock;

        public CachingDirectoryIdAccess(IDirectoryIdStreamAccess directoryIdStreamAccess, IFileSystemStatistics? fileSystemStatistics, IFileSystemHealthStatistics? fileSystemHealthStatistics)
            : base(directoryIdStreamAccess, fileSystemStatistics, fileSystemHealthStatistics)
        {
            _directoryIdCache = new(Constants.Caching.DIRECTORY_ID_CACHE_SIZE);
            _lock = new();
        }

        /// <inheritdoc/>
        public override bool GetDirectoryId(string ciphertextPath, Span<byte> directoryId)
        {
            // Check if directoryId Span is of correct length
            if (directoryId.Length != FileSystem.Constants.DIRECTORY_ID_SIZE)
                return false;

            lock (_lock)
            {
                if (!_directoryIdCache.TryGetValue(ciphertextPath, out var directoryIdBuffer))
                {
                    // Cache miss, update stats
                    fileSystemStatistics?.NotifyDirectoryIdCacheMiss();

                    // Get directory ID from file
                    if (!base.GetDirectoryId(ciphertextPath, directoryId))
                        return false;

                    // Copy the directory ID to cache
                    UpdateCache(ciphertextPath, directoryId);
                }
                else
                {
                    // Cache hit, update stats
                    fileSystemStatistics?.NotifyDirectoryIdAccess();
                    fileSystemStatistics?.NotifyDirectoryIdCacheHit();

                    directoryIdBuffer.Buffer.CopyTo(directoryId);
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public override bool SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId)
        {
            lock (_lock)
            {
                // Update directory ID in file
                if (!base.SetDirectoryId(ciphertextPath, directoryId))
                    return false;

                // Update cache after successfully setting the directory ID
                UpdateCache(ciphertextPath, directoryId);
            }

            return true;
        }

        /// <inheritdoc/>
        public override void RemoveDirectoryId(string ciphertextPath)
        {
            lock (_lock)
            {
                _ = _directoryIdCache.Remove(ciphertextPath);
            }
        }

        private void UpdateCache(string ciphertextPath, ReadOnlySpan<byte> directoryId)
        {
            // Remove first item from cache if exceeds size
            if (_directoryIdCache.Count >= Constants.Caching.DIRECTORY_ID_CACHE_SIZE)
                _directoryIdCache.Remove(_directoryIdCache.Keys.First());

            // Copy directoryId to cache
            _directoryIdCache[ciphertextPath] = new(directoryId.ToArray());
        }
    }
}
