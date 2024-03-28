using SecureFolderFS.Core.FileSystem.Statistics;
using SecureFolderFS.Shared.Enums;
using SecureFolderFS.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SecureFolderFS.Core.Directories
{
    /// <summary>
    /// Provides a cache for DirectoryIDs found on the encrypting file system.
    /// </summary>
    public sealed class DirectoryIdCache
    {
        private readonly object _lock = new();
        private readonly Dictionary<string, BufferHolder> _cache;
        private readonly IFileSystemStatistics _statistics;

        public DirectoryIdCache(IFileSystemStatistics statistics)
        {
            _statistics = statistics;
            _cache = new(FileSystem.Constants.Caching.RECOMMENDED_SIZE_DIRECTORYID);
        }

        /// <summary>
        /// Gets the DirectoryID of provided DirectoryID <paramref name="ciphertextPath"/>.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path to the DirectoryID file.</param>
        /// <param name="directoryId">The <see cref="Span{T}"/> to fill the DirectoryID into.</param>
        /// <returns>If the <paramref name="directoryId"/> was retrieved successfully; returns true; otherwise false.</returns>
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
                if (!_cache.TryGetValue(ciphertextPath, out var directoryIdBuffer))
                {
                    // Cache miss, update stats
                    _statistics.DirectoryIdCache?.Report(CacheAccessType.CacheMiss);

                    return false;
                }

                // Cache hit, update stats
                _statistics.FileNameCache?.Report(CacheAccessType.CacheAccess);
                _statistics.DirectoryIdCache?.Report(CacheAccessType.CacheHit);

                directoryIdBuffer.Buffer.CopyTo(directoryId);

                return true;
            }
        }

        /// <summary>
        /// Sets the DirectoryID of provided DirectoryID <paramref name="ciphertextPath"/> file path.
        /// </summary>
        /// <param name="ciphertextPath">The ciphertext path to the DirectoryID file.</param>
        /// <param name="directoryId">The ID to set for the directory.</param>
        public void SetDirectoryId(string ciphertextPath, ReadOnlySpan<byte> directoryId)
        {
            lock (_lock)
            {
                // Remove first item from cache if exceeds size
                if (_cache.Count >= FileSystem.Constants.Caching.RECOMMENDED_SIZE_DIRECTORYID)
                    _cache.Remove(_cache.Keys.First());

                // Copy directoryId to cache
                _cache[ciphertextPath] = new(directoryId.ToArray());
            }
        }

        /// <summary>
        /// Removes associated DirectoryID from the list of known IDs.
        /// </summary>
        /// <param name="ciphertextPath">The path associated with the DirectoryID.</param>
        public void RemoveDirectoryId(string ciphertextPath)
        {
            lock (_lock)
            {
                _ = _cache.Remove(ciphertextPath);
            }
        }
    }
}
