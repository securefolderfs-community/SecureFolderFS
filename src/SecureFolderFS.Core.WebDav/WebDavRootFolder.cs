using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav
{
    /// <inheritdoc cref="IVFSRoot"/>
    public class WebDavRootFolder : VFSRoot
    {
        protected readonly IAsyncDisposable webDavInstance;
        protected bool disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public WebDavRootFolder(IAsyncDisposable webDavInstance, IFolder storageRoot, FileSystemOptions options)
            : base(storageRoot, options)
        {
            this.webDavInstance = webDavInstance;
        }

        /// <inheritdoc/>
        public sealed override async ValueTask DisposeAsync()
        {
            if (disposed)
                return;

            disposed = true;
            await DisposeInternalAsync();
        }

        protected virtual async ValueTask DisposeInternalAsync()
        {
            await webDavInstance.DisposeAsync();
            FileSystemManager.Instance.RemoveRoot(this);
        }
    }
}
