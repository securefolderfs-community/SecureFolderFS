using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.iOS
{
    /// <inheritdoc cref="IVfsRoot"/>
    internal sealed class IOSVfsRoot : VfsRoot
    {
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.IOS.FileSystem.FS_NAME;

        public IOSVfsRoot(IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = true;
            FileSystemManager.Instance.FileSystems.Remove(this);
            await base.DisposeAsync();
        }
    }
}
