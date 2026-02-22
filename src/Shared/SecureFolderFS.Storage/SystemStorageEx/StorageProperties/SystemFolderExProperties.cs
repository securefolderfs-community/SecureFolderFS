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
    public sealed class SystemFolderExProperties : IBasicProperties, IDateProperties
    {
        private readonly DirectoryInfo _directoryInfo;

        public SystemFolderExProperties(DirectoryInfo directoryInfo)
        {
            _directoryInfo = directoryInfo;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var created = _directoryInfo.CreationTime;

            return new GenericProperty<DateTime>(created);
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            var modified = _directoryInfo.LastWriteTime;

            return new GenericProperty<DateTime>(modified);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}
