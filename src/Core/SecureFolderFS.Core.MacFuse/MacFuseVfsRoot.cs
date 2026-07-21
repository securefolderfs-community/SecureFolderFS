using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MacFuse
{
    /// <inheritdoc cref="IVfsRoot"/>
    internal sealed class MacFuseVfsRoot : VfsRoot
    {
        private readonly MacFuseWrapper _fuseWrapper;
        private bool _disposed;

        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public MacFuseVfsRoot(MacFuseWrapper fuseWrapper, IFolder virtualizedRoot, IFolder plaintextRoot, FileSystemSpecifics specifics)
            : base(virtualizedRoot, plaintextRoot, specifics)
        {
            _fuseWrapper = fuseWrapper;
        }

        /// <inheritdoc/>
        public override async ValueTask DisposeAsync()
        {
            if (_disposed)
                return;

            _disposed = await _fuseWrapper.CloseFileSystemAsync();
            if (_disposed)
            {
                FileSystemManager.Instance.FileSystems.Remove(this);
                await base.DisposeAsync();

                try
                {
                    // Remove the now-empty mount point directory. Delete is non-recursive,
                    // so a directory which is unexpectedly not empty is left untouched
                    Directory.Delete(VirtualizedRoot.Id, recursive: false);
                }
                catch (IOException)
                {
                }
                catch (UnauthorizedAccessException)
                {
                }
            }
        }
    }
}
