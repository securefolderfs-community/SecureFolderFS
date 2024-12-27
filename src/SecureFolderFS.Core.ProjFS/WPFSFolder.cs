using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;
using System;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.ProjFS
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class WPFSFolder : VFSRoot
    {
        /// <inheritdoc/>
        public override string FileSystemName { get; } = Constants.FileSystem.FS_NAME;

        public WPFSFolder(IFolder storageRoot, FileSystemOptions options)
            : base(storageRoot, options)
        {
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
