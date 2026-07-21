using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.Helpers.RecycleBin.Abstract;
using SecureFolderFS.Storage.StorageProperties;

namespace SecureFolderFS.UI.Storage.StorageProperties
{
    /// <inheritdoc cref="ISizeOfProperty"/>
    public sealed class RecycleBinSizeOfProperty : ISizeOfProperty
    {
        private readonly IModifiableFolder _recycleBin;
        private readonly FileSystemSpecifics _specifics;

        /// <inheritdoc/>
        public string Id { get; }

        /// <inheritdoc/>
        public string Name { get; }

        public RecycleBinSizeOfProperty(IModifiableFolder recycleBin, FileSystemSpecifics specifics)
        {
            _recycleBin = recycleBin;
            _specifics = specifics;
            Name = nameof(ISizeOf.SizeOf);
            Id = $"{recycleBin.Id}/{nameof(ISizeOf.SizeOf)}";
        }

        /// <inheritdoc/>
        public async Task<long?> GetValueAsync(CancellationToken cancellationToken = default)
        {
            // The semaphore-guarded overload prevents sharing violations with concurrent
            // occupied-size updates (e.g. deletes happening through the mounted file system)
            return await AbstractRecycleBinHelpers.GetOccupiedSizeAsync(_recycleBin, _specifics, cancellationToken);
        }
    }
}
