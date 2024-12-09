using System.Diagnostics;
using Android.Content;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage;
using SecureFolderFS.Storage.StorageProperties;
using SecureFolderFS.UI;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IStorableChild"/>
    internal abstract class AndroidStorable : IStorableChild, IStorableProperties, IBookmark, IWrapper<AndroidUri>
    {
        protected readonly Activity activity;
        protected readonly AndroidFolder? parent;
        protected readonly AndroidUri permissionRoot;
        protected IBasicProperties? properties;

        /// <inheritdoc/>
        public AndroidUri Inner { get; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <inheritdoc/>
        public string? BookmarkId { get; protected set; }

        /// <summary>
        /// Gets the <see cref="DocumentFile"/> associated with the storage type identified by <see cref="AndroidUri"/>.
        /// </summary>
        protected abstract DocumentFile? Document { get; }

        protected AndroidStorable(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null, string? bookmarkId = null)
        {
            this.activity = activity;
            this.parent = parent;
            this.permissionRoot = permissionRoot ?? uri;

            Inner = uri;
            BookmarkId = bookmarkId;
            Id = Inner.ToString() ?? string.Empty;
            Name = GetFileName(uri);
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(parent);
        }

        /// <inheritdoc/>
        public Task AddBookmarkAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                activity.ContentResolver?.TakePersistableUriPermission(Inner,
                    ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);

                BookmarkId = $"{Constants.STORABLE_BOOKMARK_RID}{Id}";
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task RemoveBookmarkAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                activity.ContentResolver?.ReleasePersistableUriPermission(Inner,
                    ActivityFlags.GrantWriteUriPermission | ActivityFlags.GrantReadUriPermission);

                BookmarkId = null;
            }
            catch (Exception ex)
            {
                _ = ex;
            }

            return Task.CompletedTask;
        }
        
        /// <inheritdoc/>
        public abstract Task<IBasicProperties> GetPropertiesAsync();

        protected static string? GetColumnValue(Context context, AndroidUri contentUri, string column, string? selection = null, string[]? selectionArgs = null)
        {
            try
            {
                var projection = new[] { column };
                using var cursor = context.ContentResolver!.Query(contentUri, projection, selection, selectionArgs, null);
                if (cursor?.MoveToFirst() ?? false)
                {
                    var columnIndex = cursor.GetColumnIndex(column);
                    if (columnIndex != -1)
                        return cursor.GetString(columnIndex);
                }
            }
            catch (Exception ex)
            {
                _ = ex;
                Debugger.Break();
            }

            return null;
        }

        protected static string GetFileName(AndroidUri uri)
        {
            var path = uri.Path;
            if (string.IsNullOrEmpty(path))
                return string.Empty;

            var index = path.LastIndexOf('/');
            var fileName = index == -1 ? path : path[(index + 1)..];

            return fileName.Split(':', StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;
        }
    }
}
