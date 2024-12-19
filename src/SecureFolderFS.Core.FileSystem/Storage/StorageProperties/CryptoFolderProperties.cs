using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Core.FileSystem.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    public class CryptoFolderProperties : ISizeProperties, IBasicProperties
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
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}
