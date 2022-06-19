using System;
using System.IO;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
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
        public Task<IFolder?> GetFolderFromPathAsync(string path)
        {
            if (Directory.Exists(path))
                return Task.FromResult<IFolder?>(new NativeFolder(path));

            return Task.FromResult<IFolder?>(null);
        }

        /// <inheritdoc/>
        public Task<IFile?> GetFileFromPathAsync(string path)
        {
            if (File.Exists(path))
                return Task.FromResult<IFile?>(new NativeFile(path));

            return Task.FromResult<IFile?>(null);
        }
    }
}
