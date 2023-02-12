using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

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
        public virtual string Id { get; }

        protected WindowsStorable(TStorage storage)
        {
            this.storage = storage;
            this.Id = storage.Path;
            this.Path = storage.Path;
            this.Name = storage.Name;
        }

        /// <inheritdoc/>
        public abstract Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default);
    }
}
