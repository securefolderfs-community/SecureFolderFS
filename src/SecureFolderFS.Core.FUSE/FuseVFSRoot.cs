using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class FuseVFSRoot : VFSRoot
    {
        private readonly FuseWrapper _fuseWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public FuseVFSRoot(FuseWrapper fuseWrapper, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
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
                FileSystemManager.Instance.RemoveRoot(this);
        }
    }
}