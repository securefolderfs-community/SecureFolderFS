using Android.Content;
using Android.Database;
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

            var androidUri = AndroidUri.Parse(id);
            if (androidUri is null)
                throw new FormatException("Could not parse AndroidUri from Storage ID.");

            var isFolder = IsUriFolder(androidUri, activity.ContentResolver);
            return isFolder
                ? Task.FromResult<IStorable>(new AndroidFolder(androidUri, activity))
                : Task.FromResult<IStorable>(new AndroidFile(androidUri, activity));
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
            catch (Exception ex)
            {
                _ = ex;
            }

            return Task.CompletedTask;
        }

        private static bool IsUriFolder(AndroidUri uri, ContentResolver contentResolver)
        {
            // Obtain the MIME type of the URI
            var mimeType = contentResolver.GetType(uri);

            if (mimeType != null && mimeType.StartsWith("vnd.android.document"))
                return true;
            
            // Use DocumentsContract to determine if the URI is a directory
            ICursor? cursor = null;
            try
            {
                var projection = new[] { DocumentsContract.Document.ColumnMimeType };
                cursor = contentResolver.Query(uri, projection, null, null, null);
                if (cursor is not null && cursor.MoveToFirst())
                {
                    var documentMimeType = cursor.GetString(0);
                    if (DocumentsContract.Document.MimeTypeDir.Equals(documentMimeType))
                        return true;
                }
            }
            finally
            {
                cursor?.Close();
            }
            
            return false;
        }
    }
}
