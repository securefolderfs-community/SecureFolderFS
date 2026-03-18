using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;

namespace SecureFolderFS.Core.FileSystem.Storage.StorageProperties
{
    /// <inheritdoc cref="ICreatedAtProperty"/>
    public sealed class CryptoCreatedAtProperty : ICreatedAtProperty
    {
        private readonly ICreatedAtProperty? _createdAtProperty;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public CryptoCreatedAtProperty(string id, ICreatedAtProperty? createdAtProperty)
        {
            _createdAtProperty = createdAtProperty;
            Name = nameof(ICreatedAt.CreatedAt);
            Id = $"{id}/{nameof(ICreatedAt.CreatedAt)}";
        }

        /// <inheritdoc/>
        public async Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            if (_createdAtProperty is null)
                return null;

            return await _createdAtProperty.GetValueAsync(cancellationToken);
        }
    }
}
