using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.WinUI.Storage.WindowsStorage;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class WindowsStorageService : IStorageService
    {
        /// <inheritdoc/>
        public async Task<IFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(id).AsTask(cancellationToken);
            return new WindowsStorageFolder(folder);
        }

        /// <inheritdoc/>
        public async Task<IFile> GetFileAsync(string id, CancellationToken cancellationToken = default)
        {
            var file = await StorageFile.GetFileFromPathAsync(id).AsTask(cancellationToken);
            return new WindowsStorageFile(file);
        }
    }
}
