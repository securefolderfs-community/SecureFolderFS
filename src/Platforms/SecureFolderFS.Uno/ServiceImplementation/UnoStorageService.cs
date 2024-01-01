using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Uno.Storage.WindowsStorage;
using Windows.Storage;

namespace SecureFolderFS.Uno.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class UnoStorageService : IStorageService
    {
        /// <inheritdoc/>
        public async Task<IFile> GetFileAsync(string id, CancellationToken cancellationToken = default)
        {
            id = FormatPath(id);

            var file = await StorageFile.GetFileFromPathAsync(id).AsTask(cancellationToken);
            return new UnoStorageFile(file);
        }

        /// <inheritdoc/>
        public async Task<IFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default)
        {
            id = FormatPath(id);

            var folder = await StorageFolder.GetFolderFromPathAsync(id).AsTask(cancellationToken);
            return new UnoStorageFolder(folder);
        }

        private static string FormatPath(string path)
        {
#if ANDROID
            path = path.Replace("/tree/primary:", "/storage/emulated/0/");
#endif
            
            return path;
        }
    }
}
