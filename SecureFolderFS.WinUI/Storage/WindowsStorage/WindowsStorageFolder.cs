using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.StoragePool;
using SecureFolderFS.WinUI.Storage.StoragePools;
using CreationCollisionOption = SecureFolderFS.Sdk.Storage.Enums.CreationCollisionOption;

namespace SecureFolderFS.WinUI.Storage.WindowsStorage
{
    /// <inheritdoc cref="IFolder"/>
    internal sealed class WindowsStorageFolder : WindowsBaseStorage<StorageFolder>, IFolder
    {
        private IFilePool? _filePool;
        private IFolderPool? _folderPool;

        public WindowsStorageFolder(StorageFolder storage)
            : base(storage)
        {
        }

        /// <inheritdoc/>
        public Task<IFile?> CreateFileAsync(string desiredName)
        {
            return CreateFileAsync(desiredName, CreationCollisionOption.FailIfExists);
        }

        /// <inheritdoc/>
        public async Task<IFile?> CreateFileAsync(string desiredName, CreationCollisionOption options)
        {
            try
            {
                var file = await storage.CreateFileAsync(desiredName, GetWindowsCreationCollisionOption(options));
                return new WindowsStorageFile(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<IFolder?> CreateFolderAsync(string desiredName)
        {
            return CreateFolderAsync(desiredName, CreationCollisionOption.FailIfExists);
        }

        /// <inheritdoc/>
        public async Task<IFolder?> CreateFolderAsync(string desiredName, CreationCollisionOption options)
        {
            try
            {
                var folder = await storage.CreateFolderAsync(desiredName, GetWindowsCreationCollisionOption(options));
                return new WindowsStorageFolder(folder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IFile?> GetFileAsync(string fileName)
        {
            try
            {
                var file = await storage.GetFileAsync(fileName);
                return new WindowsStorageFile(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IFolder?> GetFolderAsync(string folderName)
        {
            try
            {
                var folder = await storage.GetFolderAsync(folderName);
                return new WindowsStorageFolder(folder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFile> GetFilesAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var files = await storage.GetFilesAsync().AsTask(cancellationToken);
            if (files is null)
                yield break;

            if (cancellationToken.IsCancellationRequested)
                yield break;

            foreach (var item in files)
            {
                yield return new WindowsStorageFile(item);
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IFolder> GetFoldersAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var folders = await storage.GetFoldersAsync().AsTask(cancellationToken);
            if (folders is null)
                yield break;

            if (cancellationToken.IsCancellationRequested)
                yield break;

            foreach (var item in folders)
            {
                yield return new WindowsStorageFolder(item);
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorable> GetStorageAsync([EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var items = await storage.GetItemsAsync().AsTask(cancellationToken);
            if (items is null)
                yield break;

            foreach (var item in items)
            {
                if (cancellationToken.IsCancellationRequested)
                    yield break;

                if (item is StorageFile storageFile)
                    yield return new WindowsStorageFile(storageFile);

                if (item is StorageFolder storageFolder)
                    yield return new WindowsStorageFolder(storageFolder);
            }
        }

        /// <inheritdoc/>
        public IFilePool? GetFilePool()
        {
            if (_filePool is not null)
                return _filePool;

            var fileSystemService = Ioc.Default.GetService<IFileSystemService>();
            if (fileSystemService is null)
                return null;

            return _filePool ??= new CachingFilePool(this, fileSystemService);
        }

        /// <inheritdoc/>
        public IFolderPool? GetFolderPool()
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public override async Task<IFolder?> GetParentAsync()
        {
            try
            {
                var parent = await storage.GetParentAsync();
                return new WindowsStorageFolder(parent);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static Windows.Storage.CreationCollisionOption GetWindowsCreationCollisionOption(
            CreationCollisionOption options)
        {
            return options switch
            {
                CreationCollisionOption.GenerateUniqueName => Windows.Storage.CreationCollisionOption.GenerateUniqueName,
                CreationCollisionOption.ReplaceExisting => Windows.Storage.CreationCollisionOption.ReplaceExisting,
                CreationCollisionOption.OpenIfExists => Windows.Storage.CreationCollisionOption.OpenIfExists,
                CreationCollisionOption.FailIfExists => Windows.Storage.CreationCollisionOption.FailIfExists,
                _ => throw new ArgumentOutOfRangeException(nameof(options))
            };
        }
    }
}
