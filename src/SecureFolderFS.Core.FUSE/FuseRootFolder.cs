using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FUSE
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class FuseRootFolder : VFSRoot
    {
        private readonly FuseWrapper _fuseWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = "FUSE";

        public FuseRootFolder(FuseWrapper fuseWrapper, IFolder storageRoot, IReadWriteStatistics readWriteStatistics)
            : base(storageRoot, readWriteStatistics)
        {
            _fuseWrapper = fuseWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await _fuseWrapper.CloseFileSystemAsync(FileSystemCloseMethod.CloseForcefully);
            if (_disposed)
                FileSystemManager.Instance.RemoveRoot(this);
        }
    }
}