using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.AppModels;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <inheritdoc cref="IVFSRoot"/>
    public sealed class LocalVFSRoot : VFSRoot
    {
        private readonly IDisposable _disposable;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.LOCAL_FILE_SYSTEM_NAME;

        public LocalVFSRoot(IDisposable disposable, IFolder storageRoot, FileSystemOptions options)
            : base(storageRoot, options)
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
