using System.Runtime.CompilerServices;
using Android.Provider;
using Android.Webkit;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IFolder"/>
    internal sealed class AndroidFolder : AndroidStorable, IModifiableFolder, IChildFolder // TODO: Implement: IGetFirstByName, IGetItem
    {
        /// <inheritdoc/>
        protected override DocumentFile? Document { get; }

        public AndroidFolder(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null)
            : base(uri, activity, parent, permissionRoot)
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

            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            await DeleteContents(this);
            return;

            async Task DeleteContents(AndroidFolder storageFolder)
            {
                await foreach (var file in storageFolder.GetItemsAsync(StorableType.All, cancellationToken))
                {
                    switch (file)
                    {
                        case AndroidFolder androidFolder:
                            await DeleteContents(androidFolder);
                            break;
                        case AndroidFile androidFile when activity.ContentResolver is not null:
                            DocumentsContract.DeleteDocument(activity.ContentResolver, androidFile.Inner);
                            break;
                    }
                }

                DocumentFile.FromTreeUri(activity, storageFolder.Inner)?.Delete();
            }
        }

        /// <inheritdoc/>
        public Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var newFolder = Document?.CreateDirectory(name);
            if (newFolder is null)
                throw new UnauthorizedAccessException("Could not create a folder.");

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
                throw new UnauthorizedAccessException("Could not create a file.");

            return Task.FromResult<IChildFile>(new AndroidFile(newFile.Uri, activity, this, permissionRoot));
        }
    }
}
