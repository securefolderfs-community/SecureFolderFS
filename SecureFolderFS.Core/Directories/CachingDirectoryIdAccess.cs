using SecureFolderFS.Core.Buffers;
using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Core.FileSystem.Directories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.Directories
{
    /// <inheritdoc cref="IDirectoryIdAccess"/>
    public sealed class CachingDirectoryIdAccess : IDirectoryIdAccess
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, DirectoryIdBuffer> _directoryIdCache;
        private readonly IFileSystemStatistics? _fileSystemStatistics;

        public CachingDirectoryIdAccess(IFileSystemStatistics? fileSystemStatistics)
        {
            _fileSystemStatistics = fileSystemStatistics;
            _directoryIdCache = new(Constants.Caching.DIRECTORY_ID_CACHE_SIZE);
        }

        /// <inheritdoc/>
        public bool GetDirectoryId(string ciphertextPath, Span<byte> directoryId)
        {
            // Check if directoryId is of correct length
            if (directoryId.Length != FileSystem.Constants.DIRECTORY_ID_SIZE)
                throw new ArgumentException($"The size of {nameof(directoryId)} was too small.");

            // Check if the ciphertext path is empty
            if (string.IsNullOrEmpty(ciphertextPath))
                throw new ArgumentException($"The {nameof(ciphertextPath)} was empty.");

            lock (_lock)
            {
                if (!_directoryIdCache.TryGetValue(ciphertextPath, out var directoryIdBuffer))
                {
                    // Cache miss, update stats
                    _fileSystemStatistics?.NotifyDirectoryIdCacheMiss();

                    return false;
                }

                // Cache hit, update stats
                _fileSystemStatistics?.NotifyDirectoryIdAccess();
                _fileSystemStatistics?.NotifyDirectoryIdCacheHit();

                directoryIdBuffer.Buffer.CopyTo(directoryId);

                return true;
            }
        }

        /// <inheritdoc/>
        public void SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId)
        {
            lock (_lock)
            {
                // Remove first item from cache if exceeds size
                if (_directoryIdCache.Count >= Constants.Caching.DIRECTORY_ID_CACHE_SIZE)
                    _directoryIdCache.Remove(_directoryIdCache.Keys.First());

                // Copy directoryId to cache
                _directoryIdCache[ciphertextPath] = new(directoryId.ToArray());
            }
        }

        /// <inheritdoc/>
        public void RemoveDirectoryId(string ciphertextPath)
        {
            lock (_lock)
            {
                _ = _directoryIdCache.Remove(ciphertextPath);
            }
        }
    }
}
