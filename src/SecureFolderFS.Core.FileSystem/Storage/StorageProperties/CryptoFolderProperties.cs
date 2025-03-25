using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Core.FileSystem.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    public class CryptoFolderProperties : ISizeProperties, IDateProperties, IBasicProperties
    {
        private readonly FileSystemSpecifics _specifics;
        private readonly IBasicProperties _properties;

        public CryptoFolderProperties(FileSystemSpecifics specifics, IBasicProperties properties)
        {
            _specifics = specifics;
            _properties = properties;
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            if (_properties is not ISizeProperties sizeProperties)
                return null;

            var sizeProperty = await sizeProperties.GetSizeAsync(cancellationToken);
            if (sizeProperty is null)
                return null;

            var plaintextSize = _specifics.Security.ContentCrypt.CalculatePlaintextSize(sizeProperty.Value);
            return new GenericProperty<long>(plaintextSize);
        }
        
        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            if (_properties is not IDateProperties dateProperties)
                throw new NotSupportedException($"{nameof(IDateProperties)} is not supported.");

            return await dateProperties.GetDateCreatedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            if (_properties is not IDateProperties dateProperties)
                throw new NotSupportedException($"{nameof(IDateProperties)} is not supported.");

            return await dateProperties.GetDateModifiedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (_properties is ISizeProperties)
                yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;

            if (_properties is IDateProperties)
            {
                yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
                yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
            }
        }
    }
}
