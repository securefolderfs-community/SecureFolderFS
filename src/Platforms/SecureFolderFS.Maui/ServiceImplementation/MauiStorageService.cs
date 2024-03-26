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
        public Task<IStorable> GetFromBookmarkAsync(string id, CancellationToken cancellationToken = default)
        {
            // TODO: Implement GetBookmarkAsync method based on the platform
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task RemoveBookmark(string id, CancellationToken cancellationToken = default)
        {
#if ANDROID
            var activity = Platform.CurrentActivity;
            if (activity is not null && Android.Net.Uri.Parse(id) is var uri)
            {
                activity.ContentResolver?.ReleasePersistableUriPermission(uri,
                    Android.Content.ActivityFlags.GrantWriteUriPermission |
                    Android.Content.ActivityFlags.GrantReadUriPermission);
            }
#endif

            return Task.CompletedTask;
        }
    }
}
