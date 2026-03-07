using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Core.FileSystem.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class CryptoSizeOfProperty : ISizeOfProperty
    {
        private readonly ISizeOfProperty? _sizeOfProperty;
        private readonly FileSystemSpecifics _specifics;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public CryptoSizeOfProperty(string id, FileSystemSpecifics specifics, ISizeOfProperty? sizeOfProperty)
        {
            _specifics = specifics;
            _sizeOfProperty = sizeOfProperty;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            if (_sizeOfProperty is null)
                return null;

            var ciphertextSize = await _sizeOfProperty.GetValueAsync(cancellationToken);
            if (ciphertextSize is null)
                return null;

            return _specifics.Security.ContentCrypt.CalculatePlaintextSize(ciphertextSize.Value - _specifics.Security.HeaderCrypt.HeaderCiphertextSize);
        }
    }
}