using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Enums;
using SecureFolderFS.Sdk.Storage;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IVirtualFileSystem"/>
    internal sealed class WebDavFileSystem : IVirtualFileSystem
    {
        /// <inheritdoc/>
        public IFolder RootFolder { get; }

        /// <inheritdoc/>
        public bool IsOperational { get; }

        public WebDavFileSystem(IFolder rootFolder)
        {
            RootFolder = rootFolder;
            IsOperational = true;
        }

        /// <inheritdoc/>
        public Task<bool> CloseAsync(FileSystemCloseMethod closeMethod)
        {
            throw new NotImplementedException();
        }
    }
}
