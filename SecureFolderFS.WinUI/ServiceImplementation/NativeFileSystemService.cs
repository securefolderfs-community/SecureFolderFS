using System;
using System.IO;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.WinUI.Storage.NativeStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileSystemService"/>
    internal sealed class NativeFileSystemService : IFileSystemService
    {
        /// <inheritdoc/>
        public Task<bool> IsFileSystemAccessible()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> FileExistsAsync(string path)
        {
            if (File.Exists(path))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> DirectoryExistsAsync(string path)
        {
            if (Directory.Exists(path))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<ILocatableFolder?> GetFolderFromPathAsync(string path)
        {
            if (Directory.Exists(path))
                return Task.FromResult<ILocatableFolder?>(new NativeFolder(path));

            return Task.FromResult<ILocatableFolder?>(null);
        }

        /// <inheritdoc/>
        public Task<ILocatableFile?> GetFileFromPathAsync(string path)
        {
            if (File.Exists(path))
                return Task.FromResult<ILocatableFile?>(new NativeFile(path));

            return Task.FromResult<ILocatableFile?>(null);
        }

        /// <inheritdoc/>
        public Task<IDisposable?> ObtainLockAsync(IStorable storage)
        {
            return Task.FromResult<IDisposable?>(null); // TODO: Implement
        }
    }
}
