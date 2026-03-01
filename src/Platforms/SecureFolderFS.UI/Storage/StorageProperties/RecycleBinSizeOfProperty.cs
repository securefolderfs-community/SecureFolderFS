using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.UI.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class RecycleBinSizeOfProperty : ISizeOfProperty
    {
        private readonly IModifiableFolder _recycleBin;
        
        /// <inheritdoc/>
        public string Id { get; }
        
        /// <inheritdoc/>
        public string Name { get; }

        public RecycleBinSizeOfProperty(IModifiableFolder recycleBin)
        {
            _recycleBin = recycleBin;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{recycleBin.Id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            return await AbstractRecycleBinHelpers.GetOccupiedSizeAsync(_recycleBin, cancellationToken);
        }
    }
}
