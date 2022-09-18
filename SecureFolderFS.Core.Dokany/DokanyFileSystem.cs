using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Sdk.Storage;
using System.Threading.Tasks;
using SecureFolderFS.Core.Dokany.Callbacks;
using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.Dokany
{
    internal sealed class DokanyFileSystem : IVirtualFileSystem
    {
        private readonly DokanyWrapper _dokanyWrapper;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        public DokanyFileSystem(DokanyWrapper dokanyWrapper, IFolder rootFolder)
        {
            _dokanyWrapper = dokanyWrapper;
            RootFolder = rootFolder;
            IsOperational = true;
        }

        /// <inheritdoc/>
        public Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            IsOperational = false;
            var result = _dokanyWrapper.CloseFileSystem(closeMethod);

            return Task.FromResult(result);
        }
    }
}
