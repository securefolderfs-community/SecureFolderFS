using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.AppModels.Database
{
    /// <inheritdoc cref="BaseDatabaseModel{TDictionaryValue}"/>
    public sealed class SingleFileDatabaseModel : ObservableDatabaseModel<ISerializedModel>
    {
        private readonly string _fileName;
        private readonly IModifiableFolder _settingsFolder;
        private IFile? _databaseFile;
        private IFolderWatcher? _folderWatcher;
        private bool _canCaptureChanges;

        /// <inheritdoc/>
        protected override INotifyCollectionChanged? NotifyCollectionChanged => _folderWatcher;

        public SingleFileDatabaseModel(string fileName, IModifiableFolder settingsFolder, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _fileName = fileName;
            _settingsFolder = settingsFolder;
            _canCaptureChanges = true;
        }

        /// <inheritdoc/>
        public override Task<TValue?> GetValueAsync<TValue>(string key, Func<TValue?>? defaultValue = null, CancellationToken cancellation = default)
            where TValue : default
        {
            if (settingsCache.TryGetValue(key, out var value))
                return Task.FromResult(value.GetValue<TValue>() ?? (defaultValue is not null ? defaultValue() : default));

            var fallback = defaultValue is not null ? defaultValue() : default;
            settingsCache[key] = new NonSerializedData(fallback);

            return Task.FromResult(fallback);
        }

        /// <inheritdoc/>
        public override Task<bool> SetValueAsync<TValue>(string key, TValue? value, CancellationToken cancellation = default)
            where TValue : default
        {
            settingsCache[key] = new NonSerializedData(value);
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public override Task<bool> RemoveAsync(string key, CancellationToken cancellation = default)
        {
            var result = settingsCache.Remove(key, out _);
            return Task.FromResult(result);
        }

        /// <inheritdoc/>
        public override async Task WipeAsync(CancellationToken cancellationToken = default)
        {
            if (_databaseFile is null)
                return;

            await using var stream = await _databaseFile.OpenStreamAsync(FileAccess.Write, cancellationToken);
            stream.SetLength(0L);
            settingsCache.Clear();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await EnsureSettingsFileAsync(cancellationToken);

                _ = _databaseFile ?? throw new InvalidOperationException("The database file was not properly initialized.");

                await using var stream = await _databaseFile!.OpenReadAsync(cancellationToken);
                var settings = await serializer.DeserializeAsync<Stream, IDictionary>(stream, cancellationToken);

                // Reset the cache
                settingsCache.Clear();

                if (settings is null) // No settings saved, set cache to empty and return
                    return;

                foreach (DictionaryEntry item in settings)
                {
                    if (item.Key is not string key)
                        continue;

                    if (item.Value is ISerializedModel serializedData)
                        settingsCache[key] = serializedData;
                    else
                        settingsCache[key] = new NonSerializedData(item.Value);
                }
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await EnsureSettingsFileAsync(cancellationToken);

                _ = _databaseFile ?? throw new InvalidOperationException("The database file was not properly initialized.");

                await using var dataStream = await _databaseFile.OpenReadWriteAsync(cancellationToken);
                await using var settingsStream = await serializer.SerializeAsync<Stream, IDictionary>(settingsCache, cancellationToken);

                // Overwrite existing content
                dataStream.Position = 0L;
                dataStream.SetLength(0L);

                // Copy contents
                settingsStream.Position = 0L;
                await settingsStream.CopyToAsync(dataStream, cancellationToken);
                await dataStream.FlushAsync(cancellationToken);

                return true;
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        protected override async Task ProcessChangeAsync(string changedItem)
        {
            if (_databaseFile?.Id == changedItem)
                await InitAsync();
        }

        private async Task EnsureSettingsFileAsync(CancellationToken cancellationToken)
        {
            _databaseFile ??= await _settingsFolder.CreateFileAsync(_fileName, false, cancellationToken);
            if (_folderWatcher is null && _settingsFolder is IMutableFolder mutableFolder && _canCaptureChanges)
            {
                try
                {
                    _folderWatcher = await mutableFolder.GetFolderWatcherAsync(cancellationToken);
                    StartCapturingChanges();
                }
                catch (Exception)
                {
                    _canCaptureChanges = false;
                }
            }
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _folderWatcher?.Dispose();
            base.Dispose();
        }

        /// <inheritdoc cref="ISerializedModel"/>
        private sealed class NonSerializedData : ISerializedModel
        {
            private readonly object? _value;

            public NonSerializedData(object? value)
            {
                _value = value;
            }

            /// <inheritdoc/>
            public TValue? GetValue<TValue>()
            {
                return (TValue?)_value;
            }
        }
    }
}
