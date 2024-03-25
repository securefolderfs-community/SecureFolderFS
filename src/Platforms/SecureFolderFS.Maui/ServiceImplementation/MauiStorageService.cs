using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Maui.ServiceImplementation
{
    internal sealed class MauiStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IFolder> GetAppFolderAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder>(new SystemFolder(FileSystem.Current.AppDataDirectory));
        }

        /// <inheritdoc/>
        public Task<IStorable> GetBookmarkAsync(string id, CancellationToken cancellationToken = default)
        {
            // TODO: Implement GetBookmarkAsync method based on the platform
            throw new NotImplementedException();
        }
    }
}
