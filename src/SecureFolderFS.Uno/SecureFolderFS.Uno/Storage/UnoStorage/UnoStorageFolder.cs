using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.DirectStorage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.ExtendableStorage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Sdk.Storage.NestedStorage;
using Windows.Storage;

namespace SecureFolderFS.Uno.Storage.WindowsStorage
{
    /// <inheritdoc cref="IFolder"/>
    internal sealed class UnoStorageFolder : UnoStorable<StorageFolder>, ILocatableFolder, IFolderExtended, INestedFolder, IDirectCopy, IDirectMove
    {
        // TODO: Implement IMutableFolder

        public UnoStorageFolder(StorageFolder storage)
            : base(storage)
        {
        }

        /// <inheritdoc/>
        public async Task<INestedFile> GetFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var file = await storage.GetFileAsync(fileName).AsTask(cancellationToken);
            return new UnoStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<INestedFolder> GetFolderAsync(string folderName, CancellationToken cancellationToken = default)
        {
            var folder = await storage.GetFolderAsync(folderName).AsTask(cancellationToken);
            return new UnoStorageFolder(folder);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<INestedStorable> GetItemsAsync(StorableKind kind = StorableKind.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            switch (kind)
            {
                case StorableKind.Files:
                {
                    var files = await storage.GetFilesAsync().AsTask(cancellationToken);
                    foreach (var item in files)
                    {
                        yield return new UnoStorageFile(item);
                    }

                    break;
                }

                case StorableKind.Folders:
                {
                    var folders = await storage.GetFoldersAsync().AsTask(cancellationToken);
                    foreach (var item in folders)
                    {
                        yield return new UnoStorageFolder(item);
                    }

                    break;
                }

                case StorableKind.All:
                {
                    var items = await storage.GetItemsAsync().AsTask(cancellationToken);
                    foreach (var item in items)
                    {
                        if (item is StorageFile storageFile)
                            yield return new UnoStorageFile(storageFile);

                        if (item is StorageFolder storageFolder)
                            yield return new UnoStorageFolder(storageFolder);
                    }

                    break;
                }

                default:
                    yield break;
            }
        }

        /// <inheritdoc/>
        public Task DeleteAsync(INestedStorable item, bool permanently = default, CancellationToken cancellationToken = default)
        {
            return item switch
            {
                UnoStorable<StorageFile> storageFile => storageFile.storage
                    .DeleteAsync(GetWindowsStorageDeleteOption(permanently))
                    .AsTask(cancellationToken),

                UnoStorable<StorageFolder> storageFolder => storageFolder.storage
                    .DeleteAsync(GetWindowsStorageDeleteOption(permanently))
                    .AsTask(cancellationToken),

                _ => throw new NotImplementedException()
            };
        }

        /// <inheritdoc/>
        public async Task<INestedStorable> CreateCopyOfAsync(INestedStorable itemToCopy, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (itemToCopy is UnoStorable<StorageFile> sourceFile)
            {
                var copiedFile = await sourceFile.storage.CopyAsync(storage, itemToCopy.Name, GetWindowsNameCollisionOption(overwrite)).AsTask(cancellationToken);
                return new UnoStorageFile(copiedFile);
            }

            throw new ArgumentException($"Could not copy type {itemToCopy.GetType()}");
        }

        /// <inheritdoc/>
        public async Task<INestedStorable> MoveFromAsync(INestedStorable itemToMove, IModifiableFolder source, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            if (itemToMove is UnoStorable<StorageFile> sourceFile)
            {
                await sourceFile.storage.MoveAsync(storage, itemToMove.Name, GetWindowsNameCollisionOption(overwrite)).AsTask(cancellationToken);
                return new UnoStorageFile(sourceFile.storage);
            }

            throw new ArgumentException($"Could not copy type {itemToMove.GetType()}");
        }

        /// <inheritdoc/>
        public async Task<INestedFile> CreateFileAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            var file = await storage.CreateFileAsync(desiredName, GetWindowsCreationCollisionOption(overwrite)).AsTask(cancellationToken);
            return new UnoStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<INestedFolder> CreateFolderAsync(string desiredName, bool overwrite = default, CancellationToken cancellationToken = default)
        {
            var folder = await storage.CreateFolderAsync(desiredName, GetWindowsCreationCollisionOption(overwrite)).AsTask(cancellationToken);
            return new UnoStorageFolder(folder);
        }

        /// <inheritdoc/>
        public override async Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            var parentFolder = await storage.GetParentAsync().AsTask(cancellationToken);
            return new UnoStorageFolder(parentFolder);
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
