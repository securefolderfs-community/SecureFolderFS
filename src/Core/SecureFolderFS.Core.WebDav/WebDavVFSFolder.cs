using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IVFSRoot"/>
    public sealed class WebDavVFSFolder : VFSRoot
    {
        private readonly WebDavWrapper _webDavWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public WebDavVFSFolder(WebDavWrapper webDavWrapper, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
            _webDavWrapper = webDavWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await _webDavWrapper.CloseFileSystemAsync();
            if (_disposed)
            {
                FileSystemManager.Instance.FileSystems.Remove(this);
                await base.DisposeAsync();
            }
        }
    }
}
