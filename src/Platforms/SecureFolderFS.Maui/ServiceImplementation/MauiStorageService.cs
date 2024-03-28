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
        public Task RemoveBookmark(IStorable storable, CancellationToken cancellationToken = default)
        {
#if ANDROID
            try
            {
                var activity = Platform.CurrentActivity;
                if (activity is null)
                    return Task.CompletedTask;

                if (Android.Net.Uri.Parse(storable.Id) is var uri) // TODO: Extract actual URI instead of parsing the ID
                {
                    activity.ContentResolver?.ReleasePersistableUriPermission(uri,
                        Android.Content.ActivityFlags.GrantWriteUriPermission |
                        Android.Content.ActivityFlags.GrantReadUriPermission);
                }
            }
            catch (Exception) { }
#endif

            return Task.CompletedTask;
        }
    }
}
