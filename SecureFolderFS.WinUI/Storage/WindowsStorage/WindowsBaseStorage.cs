using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.StorageProperties;
using NameCollisionOption = SecureFolderFS.Sdk.Storage.Enums.NameCollisionOption;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IStorable"/>
    internal abstract class WindowsBaseStorage<TStorage> : IStorable
        where TStorage : class, IStorageItem
    {
        protected readonly TStorage storage;

        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

        /// <inheritdoc/>
        public IStoragePropertiesCollection? Properties => throw new NotSupportedException();

        protected WindowsBaseStorage(TStorage storage)
        {
            this.storage = storage;
            this.Path = storage.Path;
            this.Name = storage.Name;
        }

        /// <inheritdoc/>
        public virtual async Task<bool> RenameAsync(string newName, NameCollisionOption options)
        {
            try
            {
                await storage.RenameAsync(newName, (Windows.Storage.NameCollisionOption)(byte)options);

                Path = storage.Path;
                Name = storage.Name;

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DeleteAsync(bool permanently, CancellationToken cancellationToken = default)
        {
            try
            {
                await storage.DeleteAsync(permanently ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default).AsTask(cancellationToken);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public abstract Task<IFolder?> GetParentAsync();
    }
}
