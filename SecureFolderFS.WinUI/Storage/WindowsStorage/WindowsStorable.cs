using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Shared.Helpers;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IStorable"/>
    internal abstract class WindowsStorable<TStorage> : ILocatableStorable
        where TStorage : class, IStorageItem
    {
        private string? _computedId;
        internal readonly TStorage storage;

        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public virtual string Id => _computedId ??= ChecksumHelpers.CalculateChecksumForPath(Path);

        protected WindowsStorable(TStorage storage)
        {
            this.storage = storage;
            this.Path = storage.Path;
            this.Name = storage.Name;
        }

        /// <inheritdoc/>
        public abstract Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default);
    }
}
