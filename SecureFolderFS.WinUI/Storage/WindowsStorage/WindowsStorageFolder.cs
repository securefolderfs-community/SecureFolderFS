using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IFolder"/>
    internal sealed class WindowsStorageFolder : WindowsStorable<StorageFolder>, ILocatableFolder, IModifiableFolder, IFolderExtended
    {
        // TODO: Implement IMutableFolder

        public WindowsStorageFolder(StorageFolder storage)
            : base(storage)
        {
        }

        /// <inheritdoc/>
        public async Task<IFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var fileTask = storage.GetFileAsync(fileName).AsTask(cancellationToken);
            var file = await fileTask;

            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<IFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
        {
            var folderTask = storage.GetFolderAsync(folderName).AsTask(cancellationToken);
            var folder = await folderTask;

            return new WindowsStorageFolder(folder);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            switch (kind)
            {
                case StorableKind.Files:
                {
                    var files = await storage.GetFilesAsync().AsTask(cancellationToken);
                    foreach (var item in files)
                    {
                        yield return new WindowsStorageFile(item);
                    }

                    break;
                }

                case StorableKind.Folders:
                {
                    var folders = await storage.GetFoldersAsync().AsTask(cancellationToken);
                    foreach (var item in folders)
                    {
                        yield return new WindowsStorageFolder(item);
                    }

                    break;
                }

                case StorableKind.All:
                {
                    var items = await storage.GetItemsAsync().AsTask(cancellationToken);
                    foreach (var item in items)
                    {
                        if (item is StorageFile storageFile)
                            yield return new WindowsStorageFile(storageFile);

                        if (item is StorageFolder storageFolder)
                            yield return new WindowsStorageFolder(storageFolder);
                    }

                    break;
                }

                default:
                    yield break;
            }
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorable item, bool permanently = false, CancellationToken cancellationToken = default)
        {
            if (item is WindowsStorable<StorageFile> storageFile)
            {
                await storageFile.storage.DeleteAsync(GetWindowsStorageDeleteOption(permanently)).AsTask(cancellationToken);
            }
            else if (item is WindowsStorable<StorageFolder> storageFolder)
            {
                await storageFolder.storage.DeleteAsync(GetWindowsStorageDeleteOption(permanently)).AsTask(cancellationToken);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        /// <inheritdoc/>
        public async Task<IStorable> CreateCopyOfAsync(IStorable itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (itemToCopy is WindowsStorable<StorageFile> sourceFile)
            {
                var copiedFileTask = sourceFile.storage.CopyAsync(storage, itemToCopy.Name, GetWindowsNameCollisionOption(overwrite)).AsTask(cancellationToken);
                var copiedFile = await copiedFileTask;

                return new WindowsStorageFile(copiedFile);
            }

            throw new ArgumentException($"Could not copy type {itemToCopy.GetType()}");
        }

        /// <inheritdoc/>
        public async Task<IStorable> MoveFromAsync(IStorable itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (itemToMove is WindowsStorable<StorageFile> sourceFile)
            {
                await sourceFile.storage.MoveAsync(storage, itemToMove.Name, GetWindowsNameCollisionOption(overwrite)).AsTask(cancellationToken);
                return new WindowsStorageFile(sourceFile.storage);
            }

            throw new ArgumentException($"Could not copy type {itemToMove.GetType()}");
        }

        /// <inheritdoc/>
        public async Task<IFile> CreateFileAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            var fileTask = storage.CreateFileAsync(desiredName, GetWindowsCreationCollisionOption(overwrite)).AsTask(cancellationToken);
            var file = await fileTask;

            return new WindowsStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<IFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            var folderTask = storage.CreateFolderAsync(desiredName, GetWindowsCreationCollisionOption(overwrite)).AsTask(cancellationToken);
            var folder = await folderTask;

            return new WindowsStorageFolder(folder);
        }

        /// <inheritdoc/>
        public override async Task<ILocatableFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentFolderTask = storage.GetParentAsync().AsTask(cancellationToken);
            var parentFolder = await parentFolderTask;

            return new WindowsStorageFolder(parentFolder);
        }

        private static StorageDeleteOption GetWindowsStorageDeleteOption(bool permanently)
        {
            return permanently ? StorageDeleteOption.PermanentDelete : StorageDeleteOption.Default;
        }

        private static NameCollisionOption GetWindowsNameCollisionOption(bool overwrite)
        {
            return overwrite ? NameCollisionOption.ReplaceExisting : NameCollisionOption.GenerateUniqueName;
        }

        private static CreationCollisionOption GetWindowsCreationCollisionOption(bool overwrite)
        {
            return overwrite ? CreationCollisionOption.ReplaceExisting : CreationCollisionOption.OpenIfExists;
        }
    }
}
