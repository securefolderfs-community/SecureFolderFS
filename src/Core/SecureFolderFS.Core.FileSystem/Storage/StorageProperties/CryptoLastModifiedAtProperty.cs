using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Core.FileSystem.Storage.StorageProperties
{
    /// <inheritdoc cref="ILastModifiedAtProperty"/>
    public sealed class CryptoLastModifiedAtProperty : ILastModifiedAtProperty
    {
        private readonly ILastModifiedAtProperty? _lastModifiedAtProperty;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public CryptoLastModifiedAtProperty(string id, ILastModifiedAtProperty? lastModifiedAtProperty)
        {
            _lastModifiedAtProperty = lastModifiedAtProperty;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            if (_lastModifiedAtProperty is null)
                return null;

            return await _lastModifiedAtProperty.GetValueAsync(cancellationToken);
        }
    }
}
