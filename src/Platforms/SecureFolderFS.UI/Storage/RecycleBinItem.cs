using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.Storage.VirtualFileSystem;
using SecureFolderFS.UI.Storage.StorageProperties;

namespace SecureFolderFS.UI.Storage
{
    /// <inheritdoc cref="IRecycleBinItem"/>
    internal sealed class RecycleBinItem : IRecycleBinItem
    {
        private readonly IRecycleBinFolder? _recycleBin;
        private readonly RecycleBinItemDataModel _dataModel;

        /// <inheritdoc/>
        public required string Id { get; init; }

        /// <inheritdoc/>
        public required string Name { get; init; }

        /// <inheritdoc/>
        public IStorable Inner { get; }

        /// <inheritdoc/>
        public ICreatedAtProperty CreatedAt => field ??= new RecycleBinItemCreatedAtProperty(Inner.Id, _dataModel);

        /// <inheritdoc/>
        public ISizeOfProperty SizeOf => field ??= new RecycleBinItemSizeOfProperty(Inner.Id, _dataModel);

        public RecycleBinItem(IStorable inner, RecycleBinItemDataModel dataModel, IRecycleBinFolder? recycleBin)
        {
            Inner = inner;
            _dataModel = dataModel;
            _recycleBin = recycleBin;
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(_recycleBin);
        }
    }
}
