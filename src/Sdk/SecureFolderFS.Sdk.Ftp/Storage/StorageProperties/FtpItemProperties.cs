using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Sdk.Ftp.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    public sealed class FtpItemProperties : ISizeProperties, IDateProperties, IBasicProperties
    {
        private readonly string _id;
        private readonly AsyncFtpClient _ftpClient;

        public FtpItemProperties(string id, AsyncFtpClient ftpClient)
        {
            _id = id;
            _ftpClient = ftpClient;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            var size = await _ftpClient.GetFileSize(_id, -1L, cancellationToken);
            var sizeProperty = size == -1L ? null : new GenericProperty<long>(size);

            return sizeProperty;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            var info = await _ftpClient.GetObjectInfo(_id, true, cancellationToken);
            var dateProperty = new GenericProperty<DateTime>(info.Created);

            return dateProperty;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            var info = await _ftpClient.GetObjectInfo(_id, true, cancellationToken);
            var dateProperty = new GenericProperty<DateTime>(info.Modified);

            return dateProperty;
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
