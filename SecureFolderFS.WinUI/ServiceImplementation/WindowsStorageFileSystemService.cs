using System;
using System.Threading.Tasks;
using Windows.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;

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
        public async Task<ILocatableFolder?> GetFolderFromPathAsync(string path)
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
        public async Task<ILocatableFile?> GetFileFromPathAsync(string path)
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
        public Task<IDisposable?> ObtainLockAsync(IStorable storage)
        {
            return Task.FromResult<IDisposable?>(null); // TODO: Implement
        }
    }
}
