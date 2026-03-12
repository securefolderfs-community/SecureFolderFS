using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.WinFsp.Callbacks;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WinFsp
{
    /// <inheritdoc cref="IVfsRoot"/>
    internal sealed class WinFspVfsRoot : VfsRoot
    {
        private readonly WinFspHost _winFsp;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public WinFspVfsRoot(WinFspHost winFsp, IFolder virtualizedRoot, IFolder plaintextRoot, FileSystemSpecifics specifics)
            : base(virtualizedRoot, plaintextRoot, specifics)
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
