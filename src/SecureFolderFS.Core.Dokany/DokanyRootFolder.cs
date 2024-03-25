using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using System.Threading.Tasks;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.Dokany
{
    internal sealed class DokanyRootFolder : WrappedFileSystemFolder
    {
        private readonly DokanyWrapper _dokanyWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = "Dokany";

        public DokanyRootFolder(DokanyWrapper dokanyWrapper, IFolder storageRoot, IReadWriteStatistics readWriteStatistics)
            : base(storageRoot, readWriteStatistics)
        {
            _dokanyWrapper = dokanyWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (!_disposed)
                _disposed = await Task.Run(() => _dokanyWrapper.CloseFileSystem(FileSystemCloseMethod.CloseForcefully));
        }
    }
}
