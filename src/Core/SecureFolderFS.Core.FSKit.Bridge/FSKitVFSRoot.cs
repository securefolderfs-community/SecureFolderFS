using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FSKit.Bridge
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class FSKitVFSRoot : VFSRoot
    {
        private readonly FSKitHost _host;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public FSKitVFSRoot(FSKitHost host, IFolder storageRoot, FileSystemSpecifics specifics)
            : base(storageRoot, specifics)
        {
            _host = host;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            await Task.Run(_host.Dispose);
            _disposed = true;
            FileSystemManager.Instance.FileSystems.Remove(this);
            await base.DisposeAsync();
        }
    }
}

