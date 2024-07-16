using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.Core.MobileFS.Platforms.Android.FileSystem
{
    /// <inheritdoc cref="IVFSRoot"/>
    internal sealed class AndroidVFSRoot : VFSRoot
    {
        /// <inheritdoc/>
        public override string FileSystemName { get; } = "Android SAF";

        public AndroidVFSRoot(IFolder storageRoot, IReadWriteStatistics readWriteStatistics)
            : base(storageRoot, readWriteStatistics)
        {
        }

        /// <inheritdoc/>
        public override ValueTask DisposeAsync()
        {
            throw new NotImplementedException();
        }
    }
}
