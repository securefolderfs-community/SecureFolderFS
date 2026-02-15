using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.iOS.Storage.StorageProperties;
using SecureFolderFS.Storage.FileShareOptions;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage
{
    /// <inheritdoc cref="IChildFile"/>
    internal sealed class IOSFile : IOSStorable, IFileOpenShare, IChildFile
    {
        public IOSFile(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null, string? bookmarkId = null, string? suggestedName = null)
            : base(url, parent, permissionRoot, bookmarkId)
        {
            Name = !string.IsNullOrEmpty(suggestedName) ? suggestedName : Name;
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            var iosStream = accessMode switch
            {
                FileAccess.ReadWrite or FileAccess.Write => new IOSSecurityScopedStream(Inner, permissionRoot, FileAccess.ReadWrite),
                _ => new IOSSecurityScopedStream(Inner, permissionRoot, FileAccess.Read)
            };

            return Task.FromResult<Stream>(iosStream);
        }
        
        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess accessMode, FileShare shareMode, CancellationToken cancellationToken = default)
        {
            var iosStream = accessMode switch
            {
                FileAccess.ReadWrite or FileAccess.Write => new IOSSecurityScopedStream(Inner, permissionRoot, FileAccess.ReadWrite, shareMode),
                _ => new IOSSecurityScopedStream(Inner, permissionRoot, FileAccess.Read, shareMode)
            };

            return Task.FromResult<Stream>(iosStream);
        }

        /// <inheritdoc/>
        public override Task<IBasicProperties> GetPropertiesAsync()
        {
            properties ??= new IOSFileProperties(Inner, permissionRoot);
            return Task.FromResult(properties);
        }
    }
}
