using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.WinUI.Storage.NativeStorage;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class NativeStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<bool> IsAccessibleAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(File.Exists(path));
        }

        /// <inheritdoc/>
        public Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Directory.Exists(path));
        }

        /// <inheritdoc/>
        public Task<ILocatableFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"Directory for '{path}' was not found.");

            return Task.FromResult<ILocatableFolder>(new NativeFolder(path));
        }

        /// <inheritdoc/>
        public Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File for '{path}' was not found.");

            return Task.FromResult<ILocatableFile>(new NativeFile(path));
        }
    }
}
