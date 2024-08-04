using SecureFolderFS.Shared.Enums;
using System;
using System.Collections.Generic;

namespace SecureFolderFS.Core.FileSystem
{
    public class UniversalCache<TKey, TValue> : IDisposable
        where TKey : notnull
    {
        protected readonly Dictionary<TKey, TValue> cache;
        protected readonly IProgress<CacheAccessType>? cacheStatistics;
        protected readonly object threadLock = new();

        /// <summary>
        /// Determines whether caching is enabled.
        /// </summary>
        public bool IsAvailable { get; }

        public UniversalCache(bool isAvailable, IProgress<CacheAccessType>? cacheStatistics)
            : this(isAvailable ? -1 : 0, cacheStatistics)
        {
        }

        public UniversalCache(int capacity, IProgress<CacheAccessType>? cacheStatistics)
        {
            this.cache = capacity < 0 ? new() : new(capacity);
            this.cacheStatistics = cacheStatistics;
            IsAvailable = capacity > 0;
        }

        /// <summary>
        /// Gets a <typeparamref name="TValue"/> from the provided <typeparamref name="TKey"/>.
        /// </summary>
        /// <returns>If the item was found, returns a <typeparamref name="TValue"/>; otherwise null.</returns>
        public virtual TValue? CacheGet(TKey key)
        {
            if (!IsAvailable)
                return default;

            lock (threadLock)
            {
                cacheStatistics?.Report(CacheAccessType.CacheAccess);
                if (cache.TryGetValue(key, out var value))
                {
                    cacheStatistics?.Report(CacheAccessType.CacheHit);
                    return value;
                }

                cacheStatistics?.Report(CacheAccessType.CacheMiss);
                return default;
            }
        }

        /// <summary>
        /// Adds an entry to the cache.
        /// </summary>
        /// <param name="key">The key identifier in cache.</param>
        /// <param name="value">The value associated with <paramref name="key"/> in cache.</param>
        /// <param name="skipExistingCheck">Determines whether to skip checking if a key already exists.</param>
        /// <returns>If the cache was updated successfully, returns true; otherwise false.</returns>
        public virtual bool CacheSet(TKey key, TValue value, bool skipExistingCheck = false)
        {
            if (!IsAvailable)
                return false;

            lock (threadLock)
            {
                if (!skipExistingCheck)
                    return cache.TryAdd(key, value);

                cache.Add(key, value);
                return true;
            }
        }

        /// <summary>
        /// Removes an entry from the cache.
        /// </summary>
        /// <param name="key">The key identifier in cache.</param>
        /// <returns>If the cache was updated successfully, returns true; otherwise false.</returns>
        public virtual bool CacheRemove(TKey key)
        {
            lock (threadLock)
            {
                return cache.Remove(key);
            }
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            if (!IsAvailable)
                return;

            lock (threadLock)
            {
                cache.Clear();
            }
        }
    }
}
