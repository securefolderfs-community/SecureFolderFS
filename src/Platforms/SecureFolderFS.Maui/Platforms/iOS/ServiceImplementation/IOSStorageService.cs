using Foundation;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Platforms.iOS.Storage;
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
        public async Task<TStorable> GetPersistedAsync<TStorable>(string id, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            var iosUrl = GetPersistedUrl(id);
            if (iosUrl is null)
                throw new FormatException("Could not parse NSUrl from Storage ID.");

            await Task.CompletedTask;
            return (TStorable)(IStorable)(true switch
            {
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFile)) => new IOSFile(iosUrl),
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFolder)) => new IOSFolder(iosUrl),
                _ => iosUrl.HasDirectoryPath ? new IOSFolder(iosUrl) : new IOSFile(iosUrl)
            });
        }

        /// <inheritdoc/>
        public Task RemovePersistedAsync(IStorable storable, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        private static NSUrl? GetPersistedUrl(string id)
        {
            var idData = new NSData(id, NSDataBase64DecodingOptions.None);
            var url = NSUrl.FromBookmarkData(idData, NSUrlBookmarkResolutionOptions.WithoutUI, null, out var isStale, out var error);

            if (error is not null)
                throw new NSErrorException(error);

            return url;
        }
    }
}
