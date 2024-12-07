using System.Runtime.CompilerServices;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class AndroidFolderProperties : ISizeProperties, IBasicProperties
    {
        private readonly DocumentFile _document;

        public AndroidFolderProperties(DocumentFile document)
        {
            _document = document;
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            var sizeProperty = new GenericProperty<long>(_document.Length());
            return Task.FromResult<IStorageProperty<long>>(sizeProperty);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetSizeAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}

