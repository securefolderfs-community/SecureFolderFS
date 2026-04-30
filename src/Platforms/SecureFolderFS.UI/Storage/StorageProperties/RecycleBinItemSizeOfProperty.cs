using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.UI.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class RecycleBinItemSizeOfProperty : ISizeOfProperty
    {
        private readonly RecycleBinItemDataModel _dataModel;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public RecycleBinItemSizeOfProperty(string id, RecycleBinItemDataModel dataModel)
        {
            _dataModel = dataModel;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            if (_dataModel.Size < 0L)
                return Task.FromResult<long?>(null);

            return Task.FromResult(_dataModel.Size);
        }
    }
}
