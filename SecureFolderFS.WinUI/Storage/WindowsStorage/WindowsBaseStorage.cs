using System;
using System.Threading.Tasks;
using Windows.Storage;
using SecureFolderFS.Sdk.Storage;
using NameCollisionOption = SecureFolderFS.Sdk.Storage.Enums.NameCollisionOption;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IBaseStorage"/>
    internal abstract class WindowsBaseStorage<TStorage> : IBaseStorage
        where TStorage : class, IStorageItem
    {
        protected readonly TStorage storage;

        /// <inheritdoc/>
        public string Path { get; protected set; }

        /// <inheritdoc/>
        public string Name { get; protected set; }

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
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DeleteAsync(bool permanently)
        {
            try
            {
                await storage.DeleteAsync(permanently ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default);
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
