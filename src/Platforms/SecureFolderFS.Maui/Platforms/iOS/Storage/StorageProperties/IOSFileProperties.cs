using System.Runtime.CompilerServices;
using Foundation;
using Microsoft.Maui.Platform;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class IOSFileProperties : ISizeProperties, IDateProperties, IBasicProperties
    {
        private readonly NSUrl _url;

        public IOSFileProperties(NSUrl url)
        {
            _url = url;
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<long>?> GetSizeAsync(CancellationToken cancellationToken = default)
        {
            using var document = new UIDocument(_url);
            var path = document.FileUrl.Path;
            if (path is null)
                return Task.FromResult<IStorageProperty<long>?>(null);
            
            var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
            var sizeProperty = new GenericProperty<long>((long?)attributes?.Size ?? 0L);

            return Task.FromResult<IStorageProperty<long>?>(sizeProperty);
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            using var document = new UIDocument(_url);
            var path = document.FileUrl.Path;
            if (path is null)
                return Task.FromResult<IStorageProperty<DateTime>>(new GenericProperty<DateTime>(DateTime.MinValue));;
            
            var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
            if (attributes?.CreationDate is null)
                return Task.FromResult<IStorageProperty<DateTime>>(new GenericProperty<DateTime>(DateTime.MinValue));;

            var dateCreated = attributes.CreationDate.ToDateTime();
            var dateProperty = new GenericProperty<DateTime>(dateCreated);

            return Task.FromResult<IStorageProperty<DateTime>>(dateProperty);
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            using var document = new UIDocument(_url);
            var path = document.FileUrl.Path;
            if (path is null)
                return Task.FromResult<IStorageProperty<DateTime>>(new GenericProperty<DateTime>(DateTime.MinValue));;
            
            var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
            if (attributes?.CreationDate is null)
                return Task.FromResult<IStorageProperty<DateTime>>(new GenericProperty<DateTime>(DateTime.MinValue));;

            var dateModified = (attributes.ModificationDate ?? attributes.CreationDate).ToDateTime();
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
