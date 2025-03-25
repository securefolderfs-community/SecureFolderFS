using System.Runtime.CompilerServices;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class AndroidDocumentProperties : ISizeProperties, IDateProperties, IBasicProperties
    {
        private readonly DocumentFile _document;

        public AndroidDocumentProperties(DocumentFile document)
        {
            _document = document;
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            var sizeProperty = new GenericProperty<long>(_document.Length());
            return Task.FromResult<IStorageProperty<long>?>(sizeProperty);
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            // Created date is not available on Android
            return GetDateModifiedAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            var timestamp = _document.LastModified();
            var dateModified = DateTimeOffset.FromUnixTimeMilliseconds(timestamp).UtcDateTime;
            var dateProperty = new GenericProperty<DateTime>(dateModified);

            return Task.FromResult<IStorageProperty<DateTime>>(dateProperty);
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

