using OwlCore.Storage;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <inheritdoc cref="IVfsRoot"/>
    public sealed class LocalVfsRoot : VfsRoot
    {
        private readonly IDisposable _disposable;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.LOCAL_FILE_SYSTEM_NAME;

        public LocalVfsRoot(IDisposable disposable, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, storageRoot, specifics)
        {
            _disposable = disposable;
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync()
        {
            _disposable.Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
