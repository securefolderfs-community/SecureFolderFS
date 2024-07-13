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
        public async Task<TStorable> GetPersistedAsync<TStorable>(string persistableId, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            var nsUrl = GetPersistableUrl(persistableId);
            if (nsUrl is null)
                throw new FormatException($"Could not parse NSUrl from {nameof(persistableId)}.");

            var bookmarkId = persistableId.StartsWith(UI.Constants.STORABLE_BOOKMARK_RID) ? persistableId : null;
            await Task.CompletedTask;

            return (TStorable)(IStorable)(true switch
            {
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFile)) => GetFile(nsUrl, bookmarkId),
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFolder)) => new IOSFolder(nsUrl, bookmarkId: bookmarkId),
                _ => nsUrl.HasDirectoryPath ? new IOSFolder(nsUrl) : GetFile(nsUrl, bookmarkId, true)
            });

            static IStorable GetFile(NSUrl nsUrl, string? bookmarkId, bool allowDirectory = false)
            {
                if (nsUrl.FilePathUrl?.Path is { } path)
                {
                    var isDirectory = false;
                    if (!NSFileManager.DefaultManager.FileExists(path, ref isDirectory))
                        throw new FileNotFoundException(null, Path.GetFileName(path));

                    if (!NSFileManager.DefaultManager.IsReadableFile(path))
                        throw new UnauthorizedAccessException("File is not readable");

                    if (isDirectory)
                        return allowDirectory ? new IOSFolder(nsUrl, bookmarkId: bookmarkId) : throw new IOException("File is a directory.");
                }

                return new IOSFile(nsUrl, bookmarkId: bookmarkId);
            }
        }

        private static NSUrl? GetPersistableUrl(string persistableId)
        {
            if (persistableId.StartsWith(UI.Constants.STORABLE_BOOKMARK_RID))
            {
                var idData = new NSData(persistableId.Remove(0, UI.Constants.STORABLE_BOOKMARK_RID.Length), NSDataBase64DecodingOptions.None);
                var url = NSUrl.FromBookmarkData(idData, NSUrlBookmarkResolutionOptions.WithoutUI, null, out var isStale, out var error);

                if (error is not null)
                    throw new NSErrorException(error);

                return url;
            }

            return new NSUrl(persistableId);
        }
    }
}
