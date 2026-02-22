using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Storage.SystemStorageEx.StorageProperties
{
    public sealed class SystemFileExProperties : IBasicProperties, ISizeProperties, IDateProperties
    {
        private readonly FileInfo _fileInfo;

        public SystemFileExProperties(FileInfo fileInfo)
        {
            _fileInfo = fileInfo;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var size = _fileInfo.Length;

            return new GenericProperty<long>(size);
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var created = _fileInfo.CreationTime;

            return new GenericProperty<DateTime>(created);
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var modified = _fileInfo.LastWriteTime;

            return new GenericProperty<DateTime>(modified);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}
