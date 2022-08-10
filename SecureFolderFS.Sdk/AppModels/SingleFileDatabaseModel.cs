using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Extensions;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.AppModels
{
    /// <inheritdoc cref="BaseDictionaryDatabaseModel"/>
    public sealed class SingleFileDatabaseModel : BaseDictionaryDatabaseModel
    {
        private readonly IFile _databaseFile;

        public SingleFileDatabaseModel(IFile databaseFile, IAsyncSerializer<Stream> serializer)
            : base(serializer)
        {
            _databaseFile = databaseFile;
        }

        /// <inheritdoc/>
        public override async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await using var stream = await _databaseFile.TryOpenStreamAsync(FileAccess.Read, FileShare.Read, cancellationToken);
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
                _ = storageSemaphore.Release();
            }
        }

        /// <inheritdoc/>
        public override async Task<bool> SaveAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await storageSemaphore.WaitAsync(cancellationToken);
                await using var stream = await _databaseFile.TryOpenStreamAsync(FileAccess.ReadWrite, FileShare.Read, cancellationToken);
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
                _ = storageSemaphore.Release();
            }
        }
    }
}
