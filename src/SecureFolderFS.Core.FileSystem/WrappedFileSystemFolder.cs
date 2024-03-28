using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage.VirtualFileSystem;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.FileSystem
{
    public abstract class WrappedFileSystemFolder : IVFSRootFolder, IWrapper<IFolder>
    {
        /// <inheritdoc/>
        public IFolder Inner { get; }

        /// <inheritdoc/>
        public virtual string Id => Inner.Id;

        /// <inheritdoc/>
        public virtual string Name => Inner.Name;

        /// <inheritdoc/>
        public abstract string FileSystemName { get; }

        /// <inheritdoc/>
        public virtual IReadWriteStatistics ReadWriteStatistics { get; }

        protected WrappedFileSystemFolder(IFolder inner, IReadWriteStatistics readWriteStatistics)
        {
            Inner = inner;
            ReadWriteStatistics = readWriteStatistics;
        }

        /// <inheritdoc/>
        public IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, CancellationToken cancellationToken = default)
        {
            return Inner.GetItemsAsync(type, cancellationToken);
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
