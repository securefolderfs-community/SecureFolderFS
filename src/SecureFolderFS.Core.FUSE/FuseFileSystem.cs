using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.FUSE
{
    internal sealed class FuseFileSystem : IVirtualFileSystem
    {
        private readonly FuseWrapper _fuseWrapper;

        /// <inheritdoc/>
        public IFolder StorageRoot { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        public FuseFileSystem(FuseWrapper fuseWrapper, IFolder storageRoot)
        {
            _fuseWrapper = fuseWrapper;
            StorageRoot = storageRoot;
            IsOperational = true;
        }

        /// <inheritdoc/>
        public async Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            if (IsOperational)
                IsOperational = !await _fuseWrapper.CloseFileSystemAsync(closeMethod);

            return !IsOperational;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            _ = await CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }
    }
}