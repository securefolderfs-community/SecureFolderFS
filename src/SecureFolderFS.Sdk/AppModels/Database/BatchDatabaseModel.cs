using OwlCore.Storage;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels.Database
{
    /// <inheritdoc cref="BaseDatabaseModel{TDictionaryValue}"/>
    public sealed class BatchDatabaseModel : BaseDatabaseModel<BatchDatabaseModel.SettingValue>
    {
        private const string TYPE_FILE_SUFFIX = ".type";

        private readonly string _folderName;
        private readonly IModifiableFolder _settingsFolder;
        private IModifiableFolder? _databaseFolder;

        /// <summary>
        /// Gets or sets a value that determines whether or not to flush settings that are unchanged in memory.
        /// Setting to true is recommended if you don't expect others to modify the settings files.
        /// </summary>
        public bool FlushOnlyChangedValues { get; set; }

        public BatchDatabaseModel(string folderName, IModifiableFolder settingsFolder, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _folderName = folderName;
            _settingsFolder = settingsFolder;
        }

        /// <inheritdoc/>
        public override TValue? GetValue<TValue>(string key, Func<TValue>? defaultValue = null)
            where TValue : default
        {
            if (settingsCache.TryGetValue(key, out var value))
                return value.Data.TryCast(defaultValue);

            var fallback = defaultValue is not null ? defaultValue() : default;
            settingsCache[key] = new(typeof(TValue), fallback); // The data needs to be saved

            return fallback;
        }

        /// <inheritdoc/>
        public override bool SetValue<TValue>(string key, TValue? value)
            where TValue : default
        {
            if (settingsCache.ContainsKey(key))
            {
                settingsCache[key].Data = value;
                settingsCache[key].WasModified = true;
            }
            else
            {
                settingsCache[key] = new(typeof(TValue), value); // The data needs to be saved
            }

            return true;
        }

        /// <inheritdoc/>
        public override async Task LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await EnsureSettingsFolderAsync(cancellationToken);

                _ = _databaseFolder ?? throw new InvalidOperationException("The database folder was not properly initialized.");

                var allFiles = await _databaseFolder.GetFilesAsync(cancellationToken).ToListAsync(cancellationToken);
                var nonTypeFiles = allFiles.Where(x => !x.Name.Contains(TYPE_FILE_SUFFIX, StringComparison.OrdinalIgnoreCase));

                foreach (var dataFile in nonTypeFiles)
                {
                    // Find the type file with appropriate name
                    var typeFile = allFiles.FirstOrDefault(x => x.Name.Equals($"{dataFile.Name}{TYPE_FILE_SUFFIX}", StringComparison.OrdinalIgnoreCase));
                    if (typeFile is null)
                        continue;

                    try
                    {
                        // Get type string
                        var typeString = await typeFile.ReadAllTextAsync(Encoding.UTF8, cancellationToken);
                        if (string.IsNullOrEmpty(typeString))
                            continue;

                        // Get original type
                        var originalType = Type.GetType(typeString);
                        if (originalType is null)
                            continue;

                        // Open file stream and deserialize
                        await using var dataStream = await dataFile.OpenReadAsync(cancellationToken);
                        var deserialized = await serializer.DeserializeAsync(dataStream, originalType, cancellationToken);

                        // Set settings cache
                        settingsCache[dataFile.Name] = new(originalType, deserialized, false); // Data doesn't need to be saved
                    }
                    catch (Exception ex)
                    {
                        // TODO: Re-throw exceptions in some cases?
                        _ = ex;
                        Debugger.Break();
                    }
                }
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await EnsureSettingsFolderAsync(cancellationToken);

                _ = _databaseFolder ?? throw new InvalidOperationException("The database folder was not properly initialized.");

                foreach (var item in settingsCache)
                {
                    try
                    {
                        // Don't save settings whose value didn't change
                        if (FlushOnlyChangedValues && !item.Value.WasModified)
                            continue;

                        // Get files
                        var dataFile = await _databaseFolder.CreateFileAsync(item.Key, false, cancellationToken);
                        var typeFile = await _databaseFolder.CreateFileAsync($"{item.Key}{TYPE_FILE_SUFFIX}", false, cancellationToken);

                        // Data file part

                        // Open file stream and serialize
                        await using var dataStream = await dataFile.OpenReadWriteAsync(cancellationToken);
                        await using var serializedDataStream = await serializer.SerializeAsync(item.Value.Data, item.Value.Type, cancellationToken);

                        // Overwrite existing content
                        dataStream.Position = 0L;
                        dataStream.SetLength(0L);

                        // Copy contents
                        serializedDataStream.Position = 0L;
                        await serializedDataStream.CopyToAsync(dataStream, cancellationToken);

                        // Type file part

                        // Get type buffer
                        var typeBuffer = Encoding.UTF8.GetBytes(item.Value.Type.FullName ?? string.Empty);

                        // Open file stream
                        await using var typeStream = await typeFile.OpenReadWriteAsync(cancellationToken);

                        // Reset the stream
                        typeStream.Position = 0L;
                        typeStream.SetLength(0L);

                        // Write contents
                        await typeStream.WriteAsync(typeBuffer, cancellationToken);

                        // Setting saved, setting is no longer dirty
                        item.Value.WasModified = false;
                    }
                    catch (Exception ex)
                    {
                        // TODO: Re-throw exceptions in some cases?
                        _ = ex;
                        Debugger.Break();
                    }
                }
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }

        private async Task EnsureSettingsFolderAsync(CancellationToken cancellationToken)
        {
            _databaseFolder ??= (IModifiableFolder?)await _settingsFolder.CreateFolderAsync(_folderName, false, cancellationToken);
        }

        public sealed record SettingValue(Type Type, object? Data, bool WasModified = true)
        {
            public object? Data { get; set; } = Data;

            public bool WasModified { get; set; } = WasModified;
        }
    }
}
