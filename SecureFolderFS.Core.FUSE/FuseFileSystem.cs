using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.FUSE
{
    internal sealed class FuseFileSystem : IVirtualFileSystem
    {
        private readonly FuseWrapper _fuseWrapper;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        public FuseFileSystem(FuseWrapper fuseWrapper, IFolder rootFolder)
        {
            _fuseWrapper = fuseWrapper;
            RootFolder = rootFolder;
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