using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// An in-memory database model with a maximum entry limit and LRU-style eviction.
    /// When the limit is exceeded, the oldest entries are removed first.
    /// </summary>
    /// <typeparam name="TValue">The type of value stored in the database.</typeparam>
    internal sealed class InMemoryDatabaseModel<TValue> : IDatabaseModel<string>
    {
        private readonly int _maxEntries;
        private readonly Dictionary<string, TValue> _cache;
        private readonly LinkedList<string> _accessOrder; // Tracks insertion order for eviction
        private readonly object _lock = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryDatabaseModel{TValue}"/> class.
        /// </summary>
        /// <param name="maxEntries">The maximum number of entries to store. When exceeded, oldest entries are evicted.</param>
        public InMemoryDatabaseModel(int maxEntries)
        {
            _maxEntries = maxEntries;
            _cache = new Dictionary<string, TValue>();
            _accessOrder = new LinkedList<string>();
        }

        /// <inheritdoc/>
        public Task<T?> GetValueAsync<T>(string key, Func<T?>? defaultValue = null, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_cache.TryGetValue(key, out var value) && value is T typedValue)
                    return Task.FromResult<T?>(typedValue);

                return Task.FromResult(defaultValue is not null ? defaultValue() : default);
            }
        }

        /// <inheritdoc/>
        public Task<bool> SetValueAsync<T>(string key, T? value, CancellationToken cancellationToken = default)
        {
            if (value is not TValue typedValue)
                return Task.FromResult(false);

            lock (_lock)
            {
                // If key already exists, remove it from order tracking (will be re-added)
                if (_cache.ContainsKey(key))
                    _accessOrder.Remove(key);

                // Evict oldest entries if we're at capacity
                while (_accessOrder.Count >= _maxEntries && _accessOrder.First is not null)
                {
                    var oldestKey = _accessOrder.First.Value;
                    _accessOrder.RemoveFirst();
                    _cache.Remove(oldestKey);
                }

                // Add new entry
                _cache[key] = typedValue;
                _accessOrder.AddLast(key);
            }

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> RemoveAsync(string key, CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                _accessOrder.Remove(key);
                return Task.FromResult(_cache.Remove(key));
            }
        }

        /// <inheritdoc/>
        public Task WipeAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                _accessOrder.Clear();
                _cache.Clear();
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            // No initialization needed for in-memory storage
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task SaveAsync(CancellationToken cancellationToken = default)
        {
            // No persistence for in-memory storage
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (_lock)
            {
                _cache.DisposeAll();
                _accessOrder.Clear();
                _cache.Clear();
            }
        }
    }
}

