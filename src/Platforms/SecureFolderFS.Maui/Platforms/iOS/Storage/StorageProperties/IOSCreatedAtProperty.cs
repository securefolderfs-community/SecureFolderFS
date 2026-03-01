using Foundation;
using OwlCore.Storage;
using UIKit;
using Microsoft.Maui.Platform;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties
{
    /// <inheritdoc cref="ICreatedAtProperty"/>
    internal sealed class IOSCreatedAtProperty : ICreatedAtProperty
    {
        private readonly NSUrl _url;
        private readonly NSUrl _permissionRoot;
        
        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public IOSCreatedAtProperty(string id, NSUrl url, NSUrl permissionRoot)
        {
            _url = url;
            _permissionRoot = permissionRoot;
            Name = nameof(ICreatedAt.CreatedAt);
            Id = $"{id}/{nameof(ICreatedAt.CreatedAt)}";
        }

        /// <inheritdoc/>
        public Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _permissionRoot.StartAccessingSecurityScopedResource();

                using var document = new UIDocument(_url);
                var path = document.FileUrl.Path;
                if (path is null)
                    return Task.FromResult<DateTime?>(null);

                var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
                if (attributes?.CreationDate is null)
                    return Task.FromResult<DateTime?>(null);

                var dateCreated = attributes.CreationDate.ToDateTime();
                return Task.FromResult<DateTime?>(dateCreated);
            }
            finally
            {
                _permissionRoot.StopAccessingSecurityScopedResource();
            }
        }
    }
}
