using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.WinFsp.Callbacks;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WinFsp
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class WinFspVFSRoot : VFSRoot
    {
        private readonly WinFspHost _winFsp;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public WinFspVFSRoot(WinFspHost winFsp, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
            _winFsp = winFsp;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            await Task.Run(_winFsp.Dispose);
            _disposed = true;
            FileSystemManager.Instance.FileSystems.Remove(this);
            await base.DisposeAsync();
        }
    }
}
