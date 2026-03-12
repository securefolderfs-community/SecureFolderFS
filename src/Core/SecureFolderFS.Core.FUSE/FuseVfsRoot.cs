using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IVfsRoot"/>
    internal sealed class FuseVfsRoot : VfsRoot
    {
        private readonly FuseWrapper _fuseWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public FuseVfsRoot(FuseWrapper fuseWrapper, IFolder virtualizedRoot, IFolder plaintextRoot, FileSystemSpecifics specifics)
            : base(virtualizedRoot, plaintextRoot, specifics)
        {
            _fuseWrapper = fuseWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await _fuseWrapper.CloseFileSystemAsync();
            if (_disposed)
            {
                FileSystemManager.Instance.FileSystems.Remove(this);
                await base.DisposeAsync();
            }
        }
    }
}