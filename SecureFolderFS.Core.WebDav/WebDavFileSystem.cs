using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Storage;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IVirtualFileSystem"/>
    internal sealed class WebDavFileSystem : IVirtualFileSystem
    {
        private readonly WebDavWrapper _webDavWrapper;

        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; private set; }

        public WebDavFileSystem(IFolder rootFolder, WebDavWrapper webDavWrapper)
        {
            _webDavWrapper = webDavWrapper;

            RootFolder = rootFolder;
            IsOperational = true;
        }

        /// <inheritdoc/>
        public async Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            if (IsOperational)
                IsOperational = !await Task.Run(() => _webDavWrapper.CloseFileSystem(closeMethod));

            return !IsOperational;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            _ = await CloseAsync(FileSystemCloseMethod.CloseForcefully);
        }
    }
}
