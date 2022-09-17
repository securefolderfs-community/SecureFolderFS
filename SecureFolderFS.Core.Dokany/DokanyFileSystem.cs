using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Sdk.Storage;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Enums;

namespace SecureFolderFS.Core.Dokany
{
    internal sealed class DokanyFileSystem : IVirtualFileSystem
    {
        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        public DokanyFileSystem(IFolder rootFolder)
        {
            IsOperational = true;
            RootFolder = rootFolder;
        }

        /// <inheritdoc/>
        public Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            _ = closeMethod;
            IsOperational = false;

            return Task.FromResult(true);
        }
    }
}
