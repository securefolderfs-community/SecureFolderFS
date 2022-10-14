using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileSystemService"/>
    internal sealed class WindowsStorageFileSystemService : IFileSystemService
    {
        /// <inheritdoc/>
        public Task<bool> IsFileSystemAccessibleAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var file = await GetFileFromPathAsync(path, cancellationToken);
            return file is not null;
        }

        /// <inheritdoc/>
        public async Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            var folder = await GetFolderFromPathAsync(path, cancellationToken);
            return folder is not null;
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder?> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var folderTask = StorageFolder.GetFolderFromPathAsync(path).AsTask(cancellationToken);
                var folder = await folderTask;
                
                return new WindowsStorageFolder(folder);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile?> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var fileTask = StorageFile.GetFileFromPathAsync(path).AsTask(cancellationToken);
                var file = await fileTask;

                return new WindowsStorageFile(file);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <inheritdoc/>
        public Task<IDisposable?> ObtainLockAsync(IStorable storage, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IDisposable?>(null); // TODO: Implement
        }

        /// <inheritdoc/>
        public IEnumerable<string> GetFreeMountPoints()
        {
            var occupiedLetters = System.IO.Directory.GetLogicalDrives().Select(item => item[0]);
            var availableLetters = Constants.ALPHABET.ToCharArray().Skip(3).Except(occupiedLetters); // Skip floppy disk drives and system drive

            return availableLetters.Select(item => $"{item}:\\");
        }
    }
}
