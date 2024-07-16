using OwlCore.Storage;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    /// <inheritdoc cref="IVFSRoot"/>
    public abstract class VFSRoot : IVFSRoot
    {
        /// <inheritdoc/>
        public IFolder Inner { get; }

        /// <inheritdoc/>
        public abstract string FileSystemName { get; }

        /// <inheritdoc/>
        public virtual IReadWriteStatistics ReadWriteStatistics { get; }

        protected VFSRoot(IFolder storageRoot, IReadWriteStatistics readWriteStatistics)
        {
            Inner = storageRoot;
            ReadWriteStatistics = readWriteStatistics;

            // Automatically add created root
            FileSystemManager.Instance.AddRoot(this);
        }

        /// <inheritdoc/>
        public virtual void Dispose()
        {
            _ = DisposeAsync();
        }

        /// <inheritdoc/>
        public abstract ValueTask DisposeAsync();
    }
}
