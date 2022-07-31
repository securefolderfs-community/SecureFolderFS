using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="ISettingsDatabaseModel"/>
    public class DictionarySettingsDatabaseModel : ISettingsDatabaseModel, ISingleFileSerializationModel
    {
        protected readonly IAsyncSerializer<Stream> serializer;
        protected readonly SemaphoreSlim semaphore;
        protected readonly ConcurrentDictionary<string, object?> settingsCache;

        public DictionarySettingsDatabaseModel(IAsyncSerializer<Stream> serializer)
        {
            this.serializer = serializer;
            this.semaphore = new(1, 1);
            this.settingsCache = new();
        }

        /// <inheritdoc/>
        public virtual T? GetValue<T>(string key, Func<T?>? defaultValue)
        {
            if (settingsCache.TryGetValue(key, out var value))
                return (T?)value;

            var fallback = defaultValue is not null ? defaultValue() : default;
            return fallback;
        }

        /// <inheritdoc/>
        public virtual bool SetValue<T>(string key, T? value)
        {
            settingsCache[key] = value;
            return true;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> LoadAsync(IFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                await semaphore.WaitAsync(cancellationToken);
                await using var stream = await file.TryOpenStreamAsync(FileAccess.Read, cancellationToken);
                if (stream is null)
                    return false;

                var settings = await serializer.DeserializeAsync<Dictionary<string, object?>?>(stream, cancellationToken);
                settingsCache.Clear();

                if (settings is null) // No settings saved, set cache to empty and return true.
                    return true;

                foreach (var item in settings)
                {
                    settingsCache[item.Key] = item.Value;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                _ = semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> SaveAsync(IFile file, CancellationToken cancellationToken = default)
        {
            try
            {
                await semaphore.WaitAsync(cancellationToken);
                await using var stream = await file.TryOpenStreamAsync(FileAccess.ReadWrite, cancellationToken);
                if (stream is null)
                    return false;

                await using var settingsStream = await serializer.SerializeAsync<IDictionary<string, object?>>(settingsCache, cancellationToken);

                // Overwrite existing content
                stream.SetLength(0L);

                // Write settings
                await settingsStream.CopyToAsync(stream, cancellationToken);

                return true;
            }
            finally
            {
                _ = semaphore.Release();
            }
        }
    }
}
