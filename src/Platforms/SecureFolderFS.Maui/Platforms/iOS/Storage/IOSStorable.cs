using Foundation;
using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Storage;
using Constants = SecureFolderFS.UI.Constants;

namespace SecureFolderFS.Maui.Platforms.iOS.Storage
{
    /// <inheritdoc cref="IStorableChild"/>
    internal abstract class IOSStorable : IStorableChild, IBookmark, IWrapper<NSUrl>
    {
        protected readonly IOSFolder? parent;
        protected readonly NSUrl permissionRoot;

        /// <inheritdoc/>
        public NSUrl Inner { get; }

        /// <inheritdoc/>
        public virtual string Id { get; }

        /// <inheritdoc/>
        public virtual string Name { get; protected set; }

        /// <inheritdoc/>
        public string? BookmarkId { get; protected set; }

        protected IOSStorable(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null, string? bookmarkId = null)
        {
            this.parent = parent;
            this.permissionRoot = permissionRoot ?? url;

            Inner = url;
            BookmarkId = bookmarkId;

            GetImmediateProperties(url, out var id, out var name);
            Id = id ?? string.Empty;
            Name = name ?? string.Empty;
        }

        /// <inheritdoc/>
        public virtual Task<IFolder?> GetParentAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult<IFolder?>(parent);
        }

        /// <inheritdoc/>
        public virtual async Task AddBookmarkAsync(CancellationToken cancellationToken = default)
        {
            // StartAccessingSecurityScopedResource returns false for URLs that are not
            // security-scoped (e.g., items in the app sandbox) - proceed in that case,
            // and only balance with a Stop call when access was actually started
            var accessStarted = permissionRoot.StartAccessingSecurityScopedResource();
            try
            {
                var nsDataBookmark = Inner.CreateBookmarkData(NSUrlBookmarkCreationOptions.SuitableForBookmarkFile, Array.Empty<string>(), null, out var error);
                if (error is not null)
                    throw new NSErrorException(error);

                var nsEncodedBookmark = nsDataBookmark.GetBase64EncodedString(NSDataBase64EncodingOptions.None);
                BookmarkId = $"{Constants.STORABLE_BOOKMARK_RID}{nsEncodedBookmark}";
            }
            finally
            {
                if (accessStarted)
                    permissionRoot.StopAccessingSecurityScopedResource();

                await Task.CompletedTask;
            }
        }

        /// <inheritdoc/>
        public virtual Task RemoveBookmarkAsync(CancellationToken cancellationToken = default)
        {
            BookmarkId = null;
            return Task.CompletedTask;
        }

        protected static void GetImmediateProperties(NSUrl url, out string? id, out string? name)
        {
            // Resolve the path directly from the URL
            id = url.FilePathUrl?.Path ?? url.Path;
            name = Path.GetFileName(id?.TrimEnd('/')) ?? url.LastPathComponent;
        }

        protected static IStorableChild NewStorage(NSUrl url, IOSFolder? parent = null, NSUrl? permissionRoot = null)
        {
            return url.HasDirectoryPath
                ? new IOSFolder(url, parent, permissionRoot)
                : new IOSFile(url, parent, permissionRoot);
        }
    }
}
