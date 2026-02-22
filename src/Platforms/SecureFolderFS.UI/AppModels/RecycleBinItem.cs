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
        public required string Id { get; init; }

        /// <inheritdoc/>
        public required string Name { get; init; }

        /// <inheritdoc/>
        public required DateTime DeletionTimestamp { get; init; }

        /// <inheritdoc/>
        public required long Size { get; init; }

        /// <inheritdoc/>
        public IStorableChild Inner { get; }

        public RecycleBinItem(IStorableChild inner, IRecycleBinFolder? recycleBin)
        {
            Inner = inner;
            _recycleBin = recycleBin;
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(_recycleBin);
        }
    }
}
