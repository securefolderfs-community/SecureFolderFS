using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;
using NameCollisionOption = SecureFolderFS.Sdk.Storage.Enums.NameCollisionOption;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileSystemService"/>
    internal sealed class WindowsStorageFileSystemService : IFileSystemService
    {
        /// <inheritdoc/>
        public Task<bool> IsFileSystemAccessible()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> FileExistsAsync(string path)
        {
            var file = await GetFileFromPathAsync(path);

            return file is not null;
        }

        /// <inheritdoc/>
        public async Task<bool> DirectoryExistsAsync(string path)
        {
            var folder = await GetFolderFromPathAsync(path);

            return folder is not null;
        }

        /// <inheritdoc/>
        public async Task<IFolder?> GetFolderFromPathAsync(string path)
        {
            try
            {
                var folder = await StorageFolder.GetFolderFromPathAsync(path);
                return new WindowsStorageFolder(folder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<IFile?> GetFileFromPathAsync(string path)
        {
            try
            {
                var file = await StorageFile.GetFileFromPathAsync(path);
                return new WindowsStorageFile(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<TSource?> CopyAsync<TSource>(TSource source, IFolder destinationFolder, NameCollisionOption options,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default) where TSource : IBaseStorage
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public Task<TSource?> MoveAsync<TSource>(TSource source, IFolder destinationFolder, NameCollisionOption options,
            IProgress<double>? progress = null, CancellationToken cancellationToken = default) where TSource : IBaseStorage
        {
            throw new NotSupportedException();
        }
    }
}
