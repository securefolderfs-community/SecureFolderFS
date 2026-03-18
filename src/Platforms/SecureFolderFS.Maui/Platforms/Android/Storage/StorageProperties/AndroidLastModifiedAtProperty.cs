using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;

namespace SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties
{
    /// <inheritdoc cref="ILastModifiedAtProperty"/>
    internal sealed class AndroidLastModifiedAtProperty : ILastModifiedAtProperty
    {
        private readonly DocumentFile _document;
        
        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public AndroidLastModifiedAtProperty(string id, DocumentFile document)
        {
            _document = document;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
        }
        
        /// <inheritdoc/>
        public Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            var timestamp = _document.LastModified();
            var dateModified = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;

            return Task.FromResult<DateTime?>(dateModified);
        }
    }
}
