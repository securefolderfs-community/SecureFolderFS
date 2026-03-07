using Foundation;
using SecureFolderFS.Storage.StorageProperties;
using UIKit;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    internal sealed class IOSSizeOfProperty : ISizeOfProperty
    {
        private readonly NSUrl _url;
        private readonly NSUrl _permissionRoot;
        
        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public IOSSizeOfProperty(string id, NSUrl url, NSUrl permissionRoot)
        {
            _url = url;
            _permissionRoot = permissionRoot;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                _permissionRoot.StartAccessingSecurityScopedResource();
                using var document = new UIDocument(_url);
                var path = document.FileUrl.Path;
                if (path is null)
                    return Task.FromResult<long?>(null);

                var attributes = NSFileManager.DefaultManager.GetAttributes(path, out _);
                if (attributes?.Size is null)
                    return Task.FromResult<long?>(null);
                
                return Task.FromResult<long?>((long)attributes.Size);
            }
            finally
            {
                _permissionRoot.StopAccessingSecurityScopedResource();
            }
        }
    }
}
