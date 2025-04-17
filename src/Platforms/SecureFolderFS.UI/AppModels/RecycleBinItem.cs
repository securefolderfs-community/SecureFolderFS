using System;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Storage.VirtualFileSystem;

namespace SecureFolderFS.UI.AppModels
{
    /// <inheritdoc cref="IRecycleBinItem"/>
    internal sealed class RecycleBinItem : IRecycleBinItem
    {
        private readonly IRecycleBinFolder? _recycleBin;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public IStorableChild Inner { get; }

        /// <inheritdoc/>
        public DateTime DeletionTimestamp { get; }

        public RecycleBinItem(IStorableChild inner, DateTime deletionTimestamp, string? plaintextName, IRecycleBinFolder? recycleBin)
        {
            Inner = inner;
            Id = plaintextName ?? inner.Id;
            Name = plaintextName ?? inner.Name;
            DeletionTimestamp = deletionTimestamp;
            _recycleBin = recycleBin;
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(_recycleBin);
        }
    }
}
