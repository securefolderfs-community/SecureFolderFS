using Android.Content;
using Android.Database;
using Android.Provider;
using OwlCore.Storage;
using OwlCore.Storage.System.IO;
using SecureFolderFS.Maui.Platforms.Android.Storage;
using SecureFolderFS.Sdk.Services;
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
        public async Task<TStorable> GetPersistedAsync<TStorable>(string persistableId, CancellationToken cancellationToken = default)
            where TStorable : IStorable
        {
            var activity = MainActivity.Instance;
            if (activity?.ContentResolver is null)
                throw new NullReferenceException($"{nameof(activity.ContentResolver)} was null.");

            var androidUri = GetPersistableUri(persistableId);
            if (androidUri is null)
                throw new FormatException($"Could not parse AndroidUri from {nameof(persistableId)}.");

            var bookmarkId = persistableId.StartsWith(UI.Constants.STORABLE_BOOKMARK_RID) ? persistableId : null;
            await Task.CompletedTask;

            return (TStorable)(IStorable)(true switch
            {
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFile)) => new AndroidFile(androidUri, activity, bookmarkId: bookmarkId),
                _ when typeof(TStorable).IsAssignableFrom(typeof(IFolder)) => new AndroidFolder(androidUri, activity, bookmarkId: bookmarkId),
                _ => IsUriFolder(androidUri, activity.ContentResolver)
                    ? new AndroidFolder(androidUri, activity, bookmarkId: bookmarkId)
                    : new AndroidFile(androidUri, activity, bookmarkId: bookmarkId)
            });
        }

        private static AndroidUri? GetPersistableUri(string persistableId)
        {
            if (persistableId.StartsWith(UI.Constants.STORABLE_BOOKMARK_RID))
                return AndroidUri.Parse(persistableId.Remove(0, UI.Constants.STORABLE_BOOKMARK_RID.Length));

            return AndroidUri.Parse(persistableId);
        }

        private static bool IsUriFolder(AndroidUri uri, ContentResolver contentResolver) // TODO: This function is unreliable determining storage types
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
