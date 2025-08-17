using System.Runtime.CompilerServices;
using Foundation;
using Microsoft.Maui.Platform;
using OwlCore.Storage;
using SecureFolderFS.Storage.StorageProperties;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties
{
    /// <inheritdoc cref="IBasicProperties"/>
    internal sealed class IOSFolderProperties : IDateProperties, IBasicProperties
    {
        private readonly NSUrl _url;
        private readonly NSUrl _permissionRoot;

        public IOSFolderProperties(NSUrl url, NSUrl permissionRoot)
        {
            _url = url;
            _permissionRoot = permissionRoot;
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateCreatedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _permissionRoot.StartAccessingSecurityScopedResource();

                using var document = new UIDocument(_url);
                var path = document.FileUrl.Path;
                if (path is null)
                    return Task.FromResult<IStorageProperty<DateTime>>(
                        new GenericProperty<DateTime>(DateTime.MinValue));

                var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
                if (attributes?.CreationDate is null)
                    return Task.FromResult<IStorageProperty<DateTime>>(
                        new GenericProperty<DateTime>(DateTime.MinValue));

                var dateCreated = attributes.CreationDate.ToDateTime();
                var dateProperty = new GenericProperty<DateTime>(dateCreated);

                return Task.FromResult<IStorageProperty<DateTime>>(dateProperty);
            }
            finally
            {
                _permissionRoot.StopAccessingSecurityScopedResource();
            }
        }

        /// <inheritdoc/>
        public Task<IStorageProperty<DateTime>> GetDateModifiedAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _permissionRoot.StartAccessingSecurityScopedResource();

                using var document = new UIDocument(_url);
                var path = document.FileUrl.Path;
                if (path is null)
                    return Task.FromResult<IStorageProperty<DateTime>>(new GenericProperty<DateTime>(DateTime.MinValue));

                var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
                if (attributes?.CreationDate is null)
                    return Task.FromResult<IStorageProperty<DateTime>>(new GenericProperty<DateTime>(DateTime.MinValue));

                var dateModified = (attributes.ModificationDate ?? attributes.CreationDate).ToDateTime();
                var dateProperty = new GenericProperty<DateTime>(dateModified);

                return Task.FromResult<IStorageProperty<DateTime>>(dateProperty);
            }
            finally
            {
                _permissionRoot.StopAccessingSecurityScopedResource();
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorageProperty<object>> GetPropertiesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return await GetDateCreatedAsync(cancellationToken) as IStorageProperty<object>;
            yield return await GetDateModifiedAsync(cancellationToken) as IStorageProperty<object>;
        }
    }
}
