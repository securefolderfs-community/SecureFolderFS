using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="BaseDictionaryDatabaseModel{TDictionaryValue}"/>
    public sealed class MultipleFilesDatabaseModel : BaseDictionaryDatabaseModel<MultipleFilesDatabaseModel.SettingValue>
    {
        private const string TYPE_FILE_SUFFIX = ".type";

        private readonly IModifiableFolder _databaseFolder;

        /// <summary>
        /// Gets or sets a value that determines whether or not to flush settings that are unchanged in memory.
        /// Setting to true is recommended if you don't expect others to modify the settings files.
        /// </summary>
        public bool FlushOnlyChangedValues { get; set; } // TODO: This should be easily accessible when initializing settings service!

        public MultipleFilesDatabaseModel(IModifiableFolder databaseFolder, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _databaseFolder = databaseFolder;
        }

        /// <inheritdoc/>
        public override TValue? GetValue<TValue>(string key, Func<TValue?>? defaultValue = null)
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
                settingsCache[key].IsDirty = true;
            }
            else
            {
                settingsCache[key] = new(typeof(TValue), value); // The data needs to be saved
            }

            return true;
        }

        /// <inheritdoc/>
        public override async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);

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

                        // Open file stream
                        await using var dataStream = await dataFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
                        if (dataStream is null)
                            continue;

                        // Deserialize
                        var deserialized = await serializer.DeserializeAsync(dataStream, originalType, cancellationToken);

                        // Set settings cache
                        settingsCache[dataFile.Name] = new(originalType, deserialized, false); // Data doesn't need to be saved
                    }
                    catch (Exception)
                    {
                        // TODO: Re-throw exceptions in some cases?
                        continue;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // If the exception was re-thrown...
                return false;
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

                foreach (var item in settingsCache)
                {
                    try
                    {
                        // Don't save settings whose value didn't change
                        if (FlushOnlyChangedValues && !item.Value.IsDirty)
                            continue;

                        var dataFile = await _databaseFolder.TryCreateFileAsync(item.Key, false, cancellationToken);
                        var typeFile = await _databaseFolder.TryCreateFileAsync($"{item.Key}{TYPE_FILE_SUFFIX}", false, cancellationToken);
                        if (dataFile is null || typeFile is null)
                            continue;

                        // Data file part

                        // Serialize the data
                        await using var serializedDataStream = await serializer.SerializeAsync(item.Value.Data, item.Value.Type, cancellationToken);

                        // Open file stream
                        await using var dataStream = await dataFile.TryOpenStreamAsync(FileAccess.ReadWrite, FileShare.None, cancellationToken);
                        if (dataStream is null)
                            continue;

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
                        await using var typeStream = await typeFile.TryOpenStreamAsync(FileAccess.ReadWrite, FileShare.None, cancellationToken);
                        if (typeStream is null)
                            continue;

                        // Reset the stream
                        typeStream.Seek(0, SeekOrigin.Begin);
                        typeStream.SetLength(0L);

                        // Write contents
                        await typeStream.WriteAsync(typeBuffer, cancellationToken);

                        // Setting saved, setting is no longer dirty
                        item.Value.IsDirty = false;
                    }
                    catch (Exception)
                    {
                        // TODO: Re-throw exceptions in some cases?
                        continue;
                    }
                }

                return true;
            }
            catch (Exception)
            {
                // If the exception was re-thrown...
                return false;
            }
            finally
            {
                _ = storageSemaphore.Release();
            }
        }

        public sealed record SettingValue(Type Type, object? Data, bool IsDirty = true)
        {
            public object? Data { get; set; } = Data;

            public bool IsDirty { get; set; } = IsDirty;
        }
    }
}
