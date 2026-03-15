using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.DataModels;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.UI.Storage.StorageProperties
{
    public sealed class RecycleBinItemCreatedAtProperty : ICreatedAtProperty
    {
        private readonly RecycleBinItemDataModel _dataModel;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public RecycleBinItemCreatedAtProperty(string id, RecycleBinItemDataModel dataModel)
        {
            _dataModel = dataModel;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public Task<DateTime?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_dataModel.DeletionTimestamp);
        }
    }
}
