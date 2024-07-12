using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.Platforms.iOS.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class IOSStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IFolder> GetAppFolderAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder>(new SystemFolder(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory));
        }

        /// <inheritdoc/>
        public async Task<IStorable> GetFromBookmarkAsync(string id, CancellationToken cancellationToken = default)
        {
            var folder = new SystemFolder(id);
            await Task.CompletedTask;

            return folder;
        }

        /// <inheritdoc/>
        public Task RemoveBookmark(IStorable storable, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
