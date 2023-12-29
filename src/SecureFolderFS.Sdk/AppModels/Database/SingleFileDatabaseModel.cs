using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.MutableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels.Database
{
    /// <inheritdoc cref="BaseDatabaseModel{TDictionaryValue}"/>
    public sealed class SingleFileDatabaseModel : ObservableDatabaseModel<ISerializedModel>
    {
        private readonly string _fileName;
        private readonly IModifiableFolder _settingsFolder;
        private IFile? _databaseFile;
        private IFolderWatcher? _folderWatcher;

        /// <inheritdoc/>
        protected override INotifyCollectionChanged? NotifyCollectionChanged => _folderWatcher;

        public SingleFileDatabaseModel(string fileName, IModifiableFolder settingsFolder, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _fileName = fileName;
            _settingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        public override TValue? GetValue<TValue>(string key, Func<TValue>? defaultValue = null)
            where TValue : default
        {
            if (settingsCache.TryGetValue(key, out var value))
                return value.GetValue<TValue?>() ?? (defaultValue is not null ? defaultValue() : default);

            var fallback = defaultValue is not null ? defaultValue() : default;
            settingsCache[key] = new NonSerializedData(fallback);

            return fallback;
        }

        /// <inheritdoc/>
        public override bool SetValue<TValue>(string key, TValue? value)
            where TValue : default
        {
            settingsCache[key] = new NonSerializedData(value);
            return true;
        }

        /// <inheritdoc/>
        public override async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await EnsureSettingsFileAsync(cancellationToken);

                _ = _databaseFile ?? throw new InvalidOperationException("The database file was not properly initialized.");

                await using var stream = await _databaseFile!.OpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
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

                await using var dataStream = await _databaseFile.OpenStreamAsync(FileAccess.ReadWrite, FileShare.Read, cancellationToken);
                await using var settingsStream = await serializer.SerializeAsync<Stream, IDictionary>(settingsCache, cancellationToken);

                // Overwrite existing content
                dataStream.Position = 0L;
                dataStream.SetLength(0L);

                // Copy contents
                settingsStream.Position = 0L;
                await settingsStream.CopyToAsync(dataStream, cancellationToken);

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
                await LoadAsync();
        }

        private async Task EnsureSettingsFileAsync(CancellationToken cancellationToken)
        {
            _databaseFile ??= await _settingsFolder.CreateFileAsync(_fileName, false, cancellationToken);
            if (_folderWatcher is null && _settingsFolder is IMutableFolder mutableFolder)
            {
                _folderWatcher = await mutableFolder.GetFolderWatcherAsync(cancellationToken);
                StartCapturingChanges();
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
            public T? GetValue<T>()
            {
                return _value.TryCast<T>();
            }
        }
    }
}
