using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class DokanyVFSRoot : VFSRoot
    {
        private readonly DokanyWrapper _dokanyWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FILE_SYSTEM_NAME;

        public DokanyVFSRoot(DokanyWrapper dokanyWrapper, IFolder storageRoot, FileSystemOptions options)
            : base(storageRoot, options)
        {
            _dokanyWrapper = dokanyWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;
         
            _disposed = await Task.Run(() => _dokanyWrapper.CloseFileSystem(FileSystemCloseMethod.CloseForcefully));
            if (_disposed)
                FileSystemManager.Instance.RemoveRoot(this);
        }
    }
}
