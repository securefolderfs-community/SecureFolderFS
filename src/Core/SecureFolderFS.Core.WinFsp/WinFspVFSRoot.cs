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
        private readonly WinFspService _winFspService;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public WinFspVFSRoot(WinFspService winFspService, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
            _winFspService = winFspService;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await Task.Run(_winFspService.CloseFileSystem);
            if (_disposed)
            {
                FileSystemManager.Instance.FileSystems.Remove(this);
                await base.DisposeAsync();
            }
        }
    }
}
