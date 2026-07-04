using System.Runtime.CompilerServices;
using Android.Provider;
using Android.Webkit;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Storage.Renamable;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IChildFolder"/>
    internal sealed class AndroidFolder : AndroidStorable,
        IRenamableFolder,
        IChildFolder,
        IGetFirstByName,
        ICreateRenamedCopyOf,
        IMoveRenamedFrom,
        ILastModifiedAt
    {
        private static Exception RenameException { get; } = new IOException("Could not rename the item.");

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public override DocumentFile? Document { get; }

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new AndroidLastModifiedAtProperty(Id, Document ?? throw new ArgumentNullException(nameof(Document)));

        public AndroidFolder(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null, string? bookmarkId = null, string? name = null)
            : base(uri, activity, parent, permissionRoot, bookmarkId)
        {
            Document = DocumentFile.FromTreeUri(activity, uri);
            Name = name ?? Document?.Name ?? base.Name;
        }

        /// <inheritdoc/>
        public Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            switch (storable)
            {
                case AndroidFolder folder:
                {
                    if (activity.ContentResolver is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    var uri = DocumentsContract.RenameDocument(activity.ContentResolver, folder.Inner, newName);
                    if (uri is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    return Task.FromResult<IStorableChild>(new AndroidFolder(uri, activity, this, permissionRoot));
                }

                case AndroidFile file:
                {
                    if (activity.ContentResolver is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    var uri = DocumentsContract.RenameDocument(activity.ContentResolver, file.Inner, newName);
                    if (uri is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    return Task.FromResult<IStorableChild>(new AndroidFile(uri, activity, this, permissionRoot));
                }

                default: return Task.FromException<IStorableChild>(new ArgumentOutOfRangeException(nameof(storable)));
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            // Fast path: enumerate children with a single ContentResolver query.
            // DocumentFile.ListFiles() plus per-item name lookups cost several
            // ContentResolver round-trips per item, making large listings very slow
            var children = TryEnumerateChildren();
            if (children is not null)
            {
                foreach (var (uri, name, isDirectory) in children)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = (IStorableChild?)(type switch
                    {
                        StorableType.File when !isDirectory => new AndroidFile(uri, activity, this, permissionRoot, name: name),
                        StorableType.Folder when isDirectory => new AndroidFolder(uri, activity, this, permissionRoot, name: name),
                        StorableType.All => isDirectory
                            ? new AndroidFolder(uri, activity, this, permissionRoot, name: name)
                            : new AndroidFile(uri, activity, this, permissionRoot, name: name),
                        _ => null
                    });

                    if (result is not null)
                        yield return result;
                }

                yield break;
            }

            // Fallback: DocumentFile-based enumeration
            if (Document is null)
                yield break;

            var items = Document.ListFiles();
            if (items is null)
                yield break;

            foreach (var item in items)
            {
                if (item.Uri is null)
                    continue;

                var isDirectory = item.IsDirectory;
                var result = (IStorableChild?)(type switch
                {
                    StorableType.File when !isDirectory => new AndroidFile(item.Uri, activity, this, permissionRoot),
                    StorableType.Folder when isDirectory => new AndroidFolder(item.Uri, activity, this, permissionRoot),
                    StorableType.All => isDirectory
                        ? new AndroidFolder(item.Uri, activity, this, permissionRoot)
                        : new AndroidFile(item.Uri, activity, this, permissionRoot),
                    _ => null
                });

                if (result is not null)
                    yield return result;
            }

            await Task.CompletedTask.ConfigureAwait(false);
        }

        /// <summary>
        /// Enumerates the children of this folder with a single ContentResolver query.
        /// </summary>
        /// <returns>The list of children, or null when the query is unsupported or failed.</returns>
        private List<(AndroidUri Uri, string Name, bool IsDirectory)>? TryEnumerateChildren()
        {
            try
            {
                if (activity.ContentResolver is null)
                    return null;

                var documentId = DocumentsContract.IsDocumentUri(activity, Inner)
                    ? DocumentsContract.GetDocumentId(Inner)
                    : DocumentsContract.GetTreeDocumentId(Inner);

                if (documentId is null)
                    return null;

                var childrenUri = DocumentsContract.BuildChildDocumentsUriUsingTree(Inner, documentId);
                if (childrenUri is null)
                    return null;

                var projection = new[]
                {
                    DocumentsContract.Document.ColumnDocumentId,
                    DocumentsContract.Document.ColumnDisplayName,
                    DocumentsContract.Document.ColumnMimeType
                };

                using var cursor = activity.ContentResolver.Query(childrenUri, projection, null, null, null);
                if (cursor is null)
                    return null;

                var children = new List<(AndroidUri, string, bool)>(cursor.Count);
                while (cursor.MoveToNext())
                {
                    var childDocumentId = cursor.GetString(0);
                    var childName = cursor.GetString(1);
                    var childMimeType = cursor.GetString(2);
                    if (childDocumentId is null || childName is null)
                        continue;

                    var childUri = DocumentsContract.BuildDocumentUriUsingTree(Inner, childDocumentId);
                    if (childUri is null)
                        continue;

                    var isDirectory = childMimeType == DocumentsContract.Document.MimeTypeDir;
                    children.Add((childUri, childName, isDirectory));
                }

                return children;
            }
            catch (Exception)
            {
                // Not a tree-based document provider or the query failed
                return null;
            }
        }

        /// <summary>
        /// Finds a direct child by name or null when no child with the given name exists.
        /// </summary>
        private (AndroidUri Uri, string Name, bool IsDirectory)? TryFindChild(string name)
        {
            var children = TryEnumerateChildren();
            if (children is not null)
            {
                foreach (var child in children)
                {
                    if (name.Equals(child.Name, StringComparison.Ordinal))
                        return child;
                }

                return null;
            }

            // Fallback: DocumentFile-based lookup
            var file = Document?.FindFile(name);
            if (file?.Uri is null)
                return null;

            return (file.Uri, file.Name ?? name, file.IsDirectory);
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (item is not AndroidStorable androidStorable)
                throw new ArgumentException($"The {nameof(item)} is not an Android storable.", nameof(item));

            if (activity.ContentResolver is null)
                throw new UnauthorizedAccessException("Could not access Android content resolver.");

            // Fast path: DeleteDocument removes documents (including whole directory
            // trees on most providers) with a single call
            var deleted = SafetyHelpers.NoFailureResult(() => DocumentsContract.DeleteDocument(activity.ContentResolver, androidStorable.Inner));
            if (deleted)
                return;

            // Fallback: delete the folder contents recursively
            if (item is not AndroidFolder folderToDelete)
                throw new IOException($"Could not delete '{item.Name}'.");

            await DeleteContents(folderToDelete);
            return;

            async Task DeleteContents(AndroidFolder storableIsFolder)
            {
                await foreach (var itemInFolder in storableIsFolder.GetItemsAsync(StorableType.All, cancellationToken).ConfigureAwait(false))
                {
                    switch (itemInFolder)
                    {
                        case AndroidFolder androidFolder:
                            await DeleteContents(androidFolder);
                            break;

                        case AndroidFile androidFile:
                        {
                            if (!DocumentsContract.DeleteDocument(activity.ContentResolver, androidFile.Inner))
                                throw new IOException($"Could not delete '{androidFile.Name}'.");

                            break;
                        }
                    }
                }

                if (DocumentFile.FromTreeUri(activity, storableIsFolder.Inner)?.Delete() != true)
                    throw new IOException($"Could not delete '{storableIsFolder.Name}'.");
            }
        }

        /// <inheritdoc/>
        public Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var newFolder = Document?.CreateDirectory(name);
            if (newFolder?.Uri is null)
                return Task.FromException<IChildFolder>(new UnauthorizedAccessException("Could not create Android folder."));

            return Task.FromResult<IChildFolder>(new AndroidFolder(newFolder.Uri, activity, this, permissionRoot));
        }

        /// <inheritdoc/>
        public Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var mimeType = MimeTypeMap.Singleton?.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl(name)) ?? "application/octet-stream";
            var existingFile = TryFindChild(name);

            if (overwrite && existingFile is not null)
            {
                if (activity.ContentResolver is null || !DocumentsContract.DeleteDocument(activity.ContentResolver, existingFile.Value.Uri))
                    return Task.FromException<IChildFile>(new IOException($"Could not delete the existing file '{name}'."));
            }
            else if (existingFile is not null)
            {
                return Task.FromResult<IChildFile>(new AndroidFile(existingFile.Value.Uri, activity, this, permissionRoot, name: existingFile.Value.Name));
            }

            var newFile = Document?.CreateFile(mimeType, name);
            if (newFile?.Uri is null)
                return Task.FromException<IChildFile>(new UnauthorizedAccessException("Could not create Android file."));

            return Task.FromResult<IChildFile>(new AndroidFile(newFile.Uri, activity, this, permissionRoot));
        }

        /// <inheritdoc/>
        public Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, CancellationToken cancellationToken,
            CreateCopyOfDelegate fallback)
        {
            return CreateCopyOfAsync(fileToCopy, overwrite, fileToCopy.Name, cancellationToken, (mf, f, ov, _, ct) => fallback(mf, f, ov, ct));
        }

        /// <inheritdoc/>
        public Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, string newName, CancellationToken cancellationToken,
            CreateRenamedCopyOfDelegate fallback)
        {
            if (fileToCopy is not AndroidFile androidFile)
                return fallback(this, fileToCopy, overwrite, newName, cancellationToken);

            var existingFile = TryFindChild(newName);
            if (existingFile is not null)
            {
                if (!overwrite)
                    return Task.FromException<IChildFile>(new FileAlreadyExistsException(newName));

                if (activity.ContentResolver is null || !DocumentsContract.DeleteDocument(activity.ContentResolver, existingFile.Value.Uri))
                    return Task.FromException<IChildFile>(new IOException($"Could not delete the existing file '{newName}'."));
            }

            // No-op if source and destination are the same
            if (androidFile.Id == Path.Combine(Id, newName))
                return Task.FromResult<IChildFile>(androidFile);

            return CopyInternalAsync(androidFile, newName, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite, CancellationToken cancellationToken,
            MoveFromDelegate fallback)
        {
            return MoveFromAsync(fileToMove, source, overwrite, fileToMove.Name, cancellationToken, (mf, f, src, ov, _, ct) => fallback(mf, f, src, ov, ct));
        }

        /// <inheritdoc/>
        public Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite, string newName,
            CancellationToken cancellationToken, MoveRenamedFromDelegate fallback)
        {
            if (fileToMove is not AndroidFile androidFile)
                return fallback(this, fileToMove, source, overwrite, newName, cancellationToken);

            // No-op if source and destination path are identical
            if (androidFile.Id == Path.Combine(Id, newName))
                return Task.FromResult(fileToMove);

            var existingFile = TryFindChild(newName);
            if (existingFile is not null)
            {
                if (!overwrite)
                    return Task.FromException<IChildFile>(new FileAlreadyExistsException(newName));

                if (activity.ContentResolver is null || !DocumentsContract.DeleteDocument(activity.ContentResolver, existingFile.Value.Uri))
                    return Task.FromException<IChildFile>(new IOException($"Could not delete the existing file '{newName}'."));
            }

            // Fast-path: same folder means this is a pure rename
            var isSameFolder = source is AndroidFolder androidSource && androidSource.Id == Id;
            if (isSameFolder)
                return RenameInternalAsync(androidFile, newName);

            return MoveInternalAsync(androidFile, source, newName, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var child = TryFindChild(name);
            if (child is not null)
            {
                return child.Value.IsDirectory
                    ? new AndroidFolder(child.Value.Uri, activity, this, permissionRoot, name: child.Value.Name)
                    : new AndroidFile(child.Value.Uri, activity, this, permissionRoot, name: child.Value.Name);
            }

            var target = await GetItemsAsync(cancellationToken: cancellationToken)
                .FirstOrDefaultAsyncImpl(x => name.Equals(x.Name, StringComparison.Ordinal), cancellationToken)
                .ConfigureAwait(false);

            if (target is null)
                throw new FileNotFoundException($"No storage item with the name '{name}' could be found.");

            return target;
        }

        private async Task<IChildFile> CopyInternalAsync(AndroidFile source, string newName, CancellationToken cancellationToken)
        {
            if (activity.ContentResolver is null)
                throw new UnauthorizedAccessException("Could not access Android content resolver.");

            var mimeType = MimeTypeMap.Singleton?.GetMimeTypeFromExtension(MimeTypeMap.GetFileExtensionFromUrl(newName)) ?? "application/octet-stream";
            var newFile = Document?.CreateFile(mimeType, newName);
            if (newFile?.Uri is null)
                throw new UnauthorizedAccessException($"Could not create file '{newName}'.");

            await using var inputStream = activity.ContentResolver.OpenInputStream(source.Inner) ?? throw new IOException("Could not open source file for reading.");
            await using var outputStream = activity.ContentResolver.OpenOutputStream(newFile.Uri) ?? throw new IOException("Could not open destination file for writing.");
            await inputStream.CopyToAsync(outputStream, cancellationToken).ConfigureAwait(false);

            return new AndroidFile(newFile.Uri, activity, this, permissionRoot);
        }

        private Task<IChildFile> RenameInternalAsync(AndroidFile source, string newName)
        {
            if (activity.ContentResolver is null)
                return Task.FromException<IChildFile>(new UnauthorizedAccessException("Could not access Android content resolver."));

            var renamedUri = DocumentsContract.RenameDocument(activity.ContentResolver, source.Inner, newName);
            if (renamedUri is null)
                return Task.FromException<IChildFile>(RenameException);

            return Task.FromResult<IChildFile>(new AndroidFile(renamedUri, activity, this, permissionRoot));
        }

        private async Task<IChildFile> MoveInternalAsync(AndroidFile source, IModifiableFolder sourceFolder, string newName, CancellationToken cancellationToken)
        {
            // Try native move via DocumentsContract (API 26+)
            if (OperatingSystem.IsAndroidVersionAtLeast(26)
                && activity.ContentResolver is not null
                && source.Parent is { } androidSourceFolder)
            {
                var movedUri = DocumentsContract.MoveDocument(activity.ContentResolver, source.Inner, androidSourceFolder.Inner, Inner);
                if (movedUri is not null)
                {
                    // The file was moved but may still need renaming if the name changed
                    if (source.Name != newName)
                    {
                        var renamedUri = DocumentsContract.RenameDocument(activity.ContentResolver, movedUri, newName);
                        if (renamedUri is not null)
                            return new AndroidFile(renamedUri, activity, this, permissionRoot);

                        // Move succeeded, but rename failed - still return the moved file under the original name
                    }

                    return new AndroidFile(movedUri, activity, this, permissionRoot);
                }
            }

            // Fallback: copy then delete
            var copiedFile = await CopyInternalAsync(source, newName, cancellationToken).ConfigureAwait(false);
            await sourceFolder.DeleteAsync(source, cancellationToken).ConfigureAwait(false);
            return copiedFile;
        }
    }
}
