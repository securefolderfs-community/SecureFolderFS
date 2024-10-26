using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.iOS
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class IOSVFSRoot : VFSRoot
    {
        private bool _disposed;
        
        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.IOS.FileSystem.FS_NAME;

        public IOSVFSRoot(IFolder storageRoot, FileSystemOptions options)
            : base(storageRoot, options)
        {
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                FileSystemManager.Instance.RemoveRoot(this);
            }

            return ValueTask.CompletedTask;
        }
    }
}
