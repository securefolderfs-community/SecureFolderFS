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
    public class CryptoFolderProperties : IDateProperties, IBasicProperties
    {
        private readonly IBasicProperties _properties;

        public CryptoFolderProperties(IBasicProperties properties)
        {
            _properties = properties;
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
            if (_properties is IDateProperties)
            {
                yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
                yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
            }
        }
    }
}
