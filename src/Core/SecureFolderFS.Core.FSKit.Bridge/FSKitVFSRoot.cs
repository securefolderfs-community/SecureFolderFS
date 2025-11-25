using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.FSKit.Bridge
{
    internal sealed class FSKitVFSRoot : IVFSRoot
    {
        private readonly FSKitHost _host;
        private readonly FileSystemSpecifics _specifics;

        /// <inheritdoc/>
        public IFolder VirtualizedRoot { get; }

        /// <inheritdoc/>
        public string FileSystemName { get; }

        /// <inheritdoc/>
        public VirtualFileSystemOptions Options => _specifics.Options;

        public FSKitVFSRoot(FSKitHost host, IFolder virtualizedRoot, FileSystemSpecifics specifics)
        {
            _host = host;
            VirtualizedRoot = virtualizedRoot;
            _specifics = specifics;
            FileSystemName = Constants.FileSystem.FS_NAME;
        }

        /// <inheritdoc/>
        public async ValueTask DisposeAsync()
        {
            await _host.StopFileSystemAsync();
            _specifics.Dispose();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _host.StopFileSystemAsync().GetAwaiter().GetResult();
            _specifics.Dispose();
        }
    }
}

