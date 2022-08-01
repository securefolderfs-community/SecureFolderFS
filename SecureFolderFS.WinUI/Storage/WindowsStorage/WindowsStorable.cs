using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IStorable"/>
    internal abstract class WindowsStorable<TStorage> : ILocatableStorable
        where TStorage : class, IStorageItem
    {
        internal readonly TStorage storage;

        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public string Id { get; protected set; }

        protected WindowsStorable(TStorage storage)
        {
            this.storage = storage;
            this.Path = storage.Path;
            this.Name = storage.Name;
            this.Id = string.Empty;
        }

        /// <inheritdoc/>
        public abstract Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default);
    }
}
