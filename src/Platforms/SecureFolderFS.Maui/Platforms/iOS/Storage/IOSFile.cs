using Foundation;
using OwlCore.Storage;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage
{
    /// <inheritdoc cref="IChildFile"/>
    internal sealed class IOSFile : IOSStorable, IChildFile
    {
        public IOSFile(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null, string? bookmarkId = null)
            : base(url, parent, permissionRoot, bookmarkId)
        {
        }

        /// <inheritdoc/>
        public Task<Stream> OpenStreamAsync(FileAccess accessMode, CancellationToken cancellationToken = default)
        {
            var iosStream = accessMode switch
            {
                FileAccess.ReadWrite or FileAccess.Write => new IOSSecurityScopedStream(Inner, permissionRoot, FileAccess.Write),
                _ => new IOSSecurityScopedStream(Inner, permissionRoot, FileAccess.Read)
            };

            return Task.FromResult<Stream>(iosStream);
        }
    }
}
