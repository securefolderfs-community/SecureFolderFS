using Android.Content;
using Android.Provider;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Platforms.Android.Storage;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.ServiceImplementation
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class AndroidStorageService : IStorageService
    {
        /// <inheritdoc/>
        public Task<IFolder> GetAppFolderAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder>(new SystemFolder(Microsoft.Maui.Storage.FileSystem.Current.AppDataDirectory));
        }

        /// <inheritdoc/>
        public Task<IStorable> GetFromBookmarkAsync(string id, CancellationToken cancellationToken = default)
        {
            var activity = MainActivity.Instance;
            if (activity?.ContentResolver is null)
                throw new NullReferenceException($"{nameof(activity.ContentResolver)} was null.");

            var uri = AndroidUri.Parse(id);
            var mime = activity.ContentResolver.GetType(uri);
            if (mime is null)
                throw new FormatException($"Could not parse the Uri: '{id}'.");

            return mime == DocumentsContract.Document.MimeTypeDir
                ? Task.FromResult<IStorable>(new AndroidFolder(uri, activity))
                : Task.FromResult<IStorable>(new AndroidFile(uri, activity));
        }

        /// <inheritdoc/>
        public Task RemoveBookmark(IStorable storable, CancellationToken cancellationToken = default)
        {
            try
            {
                var activity = Platform.CurrentActivity;
                if (activity is null)
                    return Task.CompletedTask;

                if (storable is not IWrapper<AndroidUri> uriWrapper)
                    return Task.CompletedTask;

                activity.ContentResolver?.ReleasePersistableUriPermission(uriWrapper.Inner,
                    ActivityFlags.GrantWriteUriPermission |
                    ActivityFlags.GrantReadUriPermission);
            }
            catch (Exception)
            {
            }

            return Task.CompletedTask;
        }
    }
}
