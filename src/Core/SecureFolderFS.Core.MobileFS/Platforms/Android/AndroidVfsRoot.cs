using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android
{
    /// <inheritdoc cref="IVfsRoot"/>
    internal sealed class AndroidVfsRoot : VfsRoot
    {
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.Android.FileSystem.FS_NAME;

        public AndroidVfsRoot(IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, storageRoot, specifics)
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
