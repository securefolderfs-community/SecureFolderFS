using Android.Content;
using AndroidX.DocumentFile.Provider;
using CommunityToolkit.Maui.Core.Extensions;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IStorable"/>
    internal abstract class AndroidStorable : IStorableChild, IWrapper<AndroidUri>
    {
        protected readonly Activity activity;
        protected readonly AndroidFolder? parent;
        protected readonly AndroidUri permissionRoot;

        /// <inheritdoc/>
        public AndroidUri Inner { get; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        /// <inheritdoc/>
        public virtual string Name { get; }

        /// <summary>
        /// Gets the <see cref="DocumentFile"/> associated with the storage type identified by <see cref="AndroidUri"/>.
        /// </summary>
        protected abstract DocumentFile? Document { get; }

        protected AndroidStorable(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null)
        {
            this.activity = activity;
            this.parent = parent;
            this.permissionRoot = permissionRoot ?? uri;
            Inner = uri;
            Id = Inner.ToString() ?? string.Empty;
            Name = Path.GetFileName(Inner.ToPhysicalPath() ?? string.Empty);
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

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
                System.Diagnostics.Debugger.Break();
            }

            return null;
        }
    }
}
