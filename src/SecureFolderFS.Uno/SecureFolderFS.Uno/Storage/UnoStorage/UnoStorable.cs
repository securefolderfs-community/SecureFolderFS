using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using Windows.Storage;

namespace SecureFolderFS.Uno.Storage.WindowsStorage
{
    /// <inheritdoc cref="IStorable"/>
    internal abstract class UnoStorable<TStorage> : ILocatableStorable, INestedStorable
        where TStorage : class, IStorageItem
    {
        internal readonly TStorage storage;

        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        protected UnoStorable(TStorage storage)
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
