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
        public string StorageName { get; }

        /// <inheritdoc/>
        public abstract string FileSystemName { get; }

        /// <inheritdoc/>
        public virtual IHealthStatistics HealthStatistics { get; }

        /// <inheritdoc/>
        public virtual IFileSystemStatistics ReadWriteStatistics { get; }

        protected VFSRoot(IFolder storageRoot, FileSystemOptions options)
        {
            Inner = storageRoot;
            StorageName = options.VolumeName;
            HealthStatistics = options.HealthStatistics;
            ReadWriteStatistics = options.FileSystemStatistics;

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
