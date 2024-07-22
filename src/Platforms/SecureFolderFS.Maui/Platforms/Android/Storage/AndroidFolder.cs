using System.Runtime.CompilerServices;
using Android.Provider;
using Android.Webkit;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IChildFolder"/>
    internal sealed class AndroidFolder : AndroidStorable, IModifiableFolder, IChildFolder, IGetFirstByName // TODO: Implement: IGetFirstByName, IGetItem
    {
        /// <inheritdoc/>
        protected override DocumentFile? Document { get; }

        public AndroidFolder(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null, string? bookmarkId = null)
            : base(uri, activity, parent, permissionRoot, bookmarkId)
        {
            Document = DocumentFile.FromTreeUri(activity, uri);
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var contentResolver = activity.ContentResolver;
            if (contentResolver is null)
                yield break;

            var folderId = permissionRoot != Inner ? DocumentsContract.GetDocumentId(Inner) : DocumentsContract.GetTreeDocumentId(Inner);
            var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(permissionRoot, folderId);

            var projection = new[]
            {
                DocumentsContract.Document.ColumnDocumentId,
                DocumentsContract.Document.ColumnMimeType
            };
            if (childrenUri != null)
            {
                using var cursor = contentResolver.Query(childrenUri, projection, null, null, null);
                if (cursor is null)
                    yield break;

                while (cursor.MoveToNext())
                {
                    var id = cursor.GetString(0);
                    var mime = cursor.GetString(1);
                    var isDirectory = mime == DocumentsContract.Document.MimeTypeDir;

                    var uri = DocumentsContract.BuildDocumentUriUsingTree(permissionRoot, id);
                    if (uri is null)
                        continue;

                    switch (type)
                    {
                        case StorableType.File:
                            if (!isDirectory)
                                yield return new AndroidFile(uri, activity, this, permissionRoot);

                            break;

                        case StorableType.Folder:
                            if (isDirectory)
                                yield return new AndroidFolder(uri, activity, this, permissionRoot);

                            break;

                        case StorableType.All:
                            yield return isDirectory
                                ? new AndroidFolder(uri, activity, this, permissionRoot)
                                : new AndroidFile(uri, activity, this, permissionRoot);
                            break;

                        default:
                        case StorableType.None:
                            yield break;
                    }
                }
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            await DeleteContents(item);
            return;

            async Task DeleteContents(IStorableChild storable)
            {
                switch (storable)
                {
                    case AndroidFile storableIsFile:
                    {
                        if (activity.ContentResolver is not null)
                            DocumentsContract.DeleteDocument(activity.ContentResolver, storableIsFile.Inner);

                        break;
                    }

                    case AndroidFolder storableIsFolder:
                    {
                        await foreach (var itemInFolder in storableIsFolder.GetItemsAsync(StorableType.All, cancellationToken).ConfigureAwait(false))
                        {
                            switch (itemInFolder)
                            {
                                case AndroidFolder androidFolder:
                                    await DeleteContents(androidFolder);
                                    break;

                                case AndroidFile androidFile when activity.ContentResolver is not null:
                                    DocumentsContract.DeleteDocument(activity.ContentResolver, androidFile.Inner);
                                    break;
                            }
                        }

                        DocumentFile.FromTreeUri(activity, storableIsFolder.Inner)?.Delete();
                        break;
                    }
                }
            }
        }

        /// <inheritdoc/>
        public Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var newFolder = Document?.CreateDirectory(name);
            if (newFolder is null)
                return Task.FromException<IChildFolder>(new UnauthorizedAccessException("Could not create Android folder."));

            return Task.FromResult<IChildFolder>(new AndroidFolder(newFolder.Uri, activity, this, permissionRoot));
        }

        /// <inheritdoc/>
        public Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var mimeType = MimeTypeMap.Singleton?.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl(name)) ?? "application/octet-stream";
            var existingFile = Document?.FindFile(name);

            if (overwrite && existingFile is not null)
                existingFile.Delete();

            var newFile = Document?.CreateFile(mimeType, name);
            if (newFile is null)
                return Task.FromException<IChildFile>(new UnauthorizedAccessException("Could not create Android file."));

            return Task.FromResult<IChildFile>(new AndroidFile(newFile.Uri, activity, this, permissionRoot));
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var file = Document?.FindFile(name);
            if (file is not null)
            {
                if (file.IsFile)
                    return new AndroidFile(file.Uri, activity, this, permissionRoot);

                if (file.IsDirectory)
                    return new AndroidFolder(file.Uri, activity, this, permissionRoot);

                throw new InvalidOperationException("The found item is neither a file nor a folder.");
            }

            var target = await GetItemsAsync(cancellationToken: cancellationToken)
                .FirstOrDefaultAsync(x => name.Equals(x.Name, StringComparison.Ordinal), cancellationToken)
                .ConfigureAwait(false);

            if (target is null)
                throw new DirectoryNotFoundException($"No storage item with the name '{name}' could be found.");

            return target;
        }
    }
}
