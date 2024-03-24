using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Dokany
{
    /// <inheritdoc cref="IVirtualFileSystem"/>
    internal sealed class DokanyFileSystem : IVirtualFileSystem
    {
        private readonly DokanyWrapper _dokanyWrapper;

        /// <inheritdoc/>
        public IFolder StorageRoot { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        public DokanyFileSystem(DokanyWrapper dokanyWrapper, IFolder storageRoot)
        {
            _dokanyWrapper = dokanyWrapper;
            StorageRoot = storageRoot;
            IsOperational = true;
        }

        /// <inheritdoc/>
        public async Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            if (IsOperational)
                IsOperational = !await Task.Run(() => _dokanyWrapper.CloseFileSystem(closeMethod));

            return !IsOperational;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            _ = await CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }
    }
}
