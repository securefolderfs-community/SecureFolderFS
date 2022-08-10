using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <summary>
    /// Represents a dictionary-based database model.
    /// </summary>
    public abstract class BaseDictionaryDatabaseModel : IDatabaseModel<string>
    {
        protected readonly IAsyncSerializer<Stream> serializer;
        protected readonly SemaphoreSlim storageSemaphore;
        protected readonly ConcurrentDictionary<string, object?> settingsCache;

        protected BaseDictionaryDatabaseModel(IAsyncSerializer<Stream> serializer)
        {
            this.serializer = serializer;
            this.storageSemaphore = new(1, 1);
            this.settingsCache = new();
        }

        /// <inheritdoc/>
        public virtual TValue? GetValue<TValue>(string key, Func<TValue?>? defaultValue)
        {
            if (settingsCache.TryGetValue(key, out var value))
                return (TValue?)value;

            var fallback = defaultValue is not null ? defaultValue() : default;
            return fallback;
        }

        /// <inheritdoc/>
        public virtual bool SetValue<TValue>(string key, TValue? value)
        {
            settingsCache[key] = value;
            return true;
        }

        /// <inheritdoc/>
        public abstract Task<bool> LoadAsync(CancellationToken cancellationToken = default);

        /// <inheritdoc/>
        public abstract Task<bool> SaveAsync(CancellationToken cancellationToken = default);
    }
}
