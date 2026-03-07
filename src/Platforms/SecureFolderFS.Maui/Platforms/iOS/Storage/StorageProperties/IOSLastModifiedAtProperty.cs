using Foundation;
using OwlCore.Storage;
using UIKit;
using Microsoft.Maui.Platform;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties
{
    /// <inheritdoc cref="ILastModifiedAtProperty"/>
    internal sealed class IOSLastModifiedAtProperty : ILastModifiedAtProperty
    {
        private readonly NSUrl _url;
        private readonly NSUrl _permissionRoot;
        
        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public IOSLastModifiedAtProperty(string id, NSUrl url, NSUrl permissionRoot)
        {
            _url = url;
            _permissionRoot = permissionRoot;
            Name = nameof(ILastModifiedAt.LastModifiedAt);
            Id = $"{id}/{nameof(ILastModifiedAt.LastModifiedAt)}";
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
                if (attributes?.ModificationDate is null)
                    return Task.FromResult<DateTime?>(null);

                var dateModified = attributes.ModificationDate.ToDateTime();
                return Task.FromResult<DateTime?>(dateModified);
            }
            finally
            {
                _permissionRoot.StopAccessingSecurityScopedResource();
            }
        }
    }
}
