using System.Runtime.CompilerServices;
using Android.Provider;
using Android.Webkit;
using AndroidX.DocumentFile.Provider;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.StorageProperties;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IChildFolder"/>
    internal sealed class AndroidFolder : AndroidStorable, IChildFolder, IGetFirstByName, IRenamableFolder, ICreateRenamedCopyOf, IMoveRenamedFrom
    {
        private static Exception RenameException { get; } = new IOException("Could not rename the item.");

        /// <inheritdoc/>
        public override string Name { get; }

        /// <inheritdoc/>
        public override DocumentFile? Document { get; }

        public AndroidFolder(AndroidUri uri, Activity activity, AndroidFolder? parent = null, AndroidUri? permissionRoot = null, string? bookmarkId = null)
            : base(uri, activity, parent, permissionRoot, bookmarkId)
        {
            Document = DocumentFile.FromTreeUri(activity, uri);
            Name = Document?.Name ?? base.Name;
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

                    return Task.FromResult<IStorableChild>(new AndroidFolder(uri, activity, Parent, permissionRoot));
                }

                case AndroidFile file:
                {
                    if (activity.ContentResolver is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    var uri = DocumentsContract.RenameDocument(activity.ContentResolver, file.Inner, newName);
                    if (uri is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    return Task.FromResult<IStorableChild>(new AndroidFile(uri, activity, Parent, permissionRoot));
                }

                default: return Task.FromException<IStorableChild>(new ArgumentOutOfRangeException(nameof(storable)));
            }
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (Document is null)
                yield break;

            foreach (var item in Document.ListFiles())
            {
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

            if (overwrite && existingFile?.Uri is not null)
            {
                existingFile.Delete();
            }
            else if (existingFile?.Uri is not null)
            {
                return Task.FromResult<IChildFile>(new AndroidFile(existingFile.Uri, activity, this, permissionRoot));
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

            var existingFile = Document?.FindFile(newName);
            if (existingFile is not null)
            {
                if (!overwrite)
                    return Task.FromException<IChildFile>(new FileAlreadyExistsException(newName));

                existingFile.Delete();
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

            var existingFile = Document?.FindFile(newName);
            if (existingFile is not null)
            {
                if (!overwrite)
                    return Task.FromException<IChildFile>(new FileAlreadyExistsException(newName));

                existingFile.Delete();
            }

            // No-op if source and destination are the same
            if (androidFile.Id == Path.Combine(Id, newName))
                return Task.FromResult<IChildFile>(fileToMove);

            return MoveInternalAsync(androidFile, source, newName, cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var file = Document?.FindFile(name);
            if (file?.Uri is not null)
            {
                if (file.IsFile)
                    return new AndroidFile(file.Uri, activity, this, permissionRoot);

                if (file.IsDirectory)
                    return new AndroidFolder(file.Uri, activity, this, permissionRoot);

                throw new InvalidOperationException("The found item is neither a file nor a folder.");
            }

            var target = await GetItemsAsync(cancellationToken: cancellationToken)
                .FirstOrDefaultAsyncImpl(x => name.Equals(x.Name, StringComparison.Ordinal), cancellationToken)
                .ConfigureAwait(false);

            if (target is null)
                throw new FileNotFoundException($"No storage item with the name '{name}' could be found.");

            return target;
        }

        /// <inheritdoc/>
        public override Task<IBasicProperties> GetPropertiesAsync()
        {
            if (Document is null)
                return Task.FromException<IBasicProperties>(new ArgumentNullException(nameof(Document)));

            properties ??= new AndroidFolderProperties(Document);
            return Task.FromResult(properties);
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

        private async Task<IChildFile> MoveInternalAsync(AndroidFile source, IModifiableFolder sourceFolder, string newName, CancellationToken cancellationToken)
        {
            // First try native move via DocumentsContract (API 26+)
            if (OperatingSystem.IsAndroidVersionAtLeast(26)
                && activity.ContentResolver is not null
                && source.Parent is { } androidSourceFolder)
            {
                var movedUri = DocumentsContract.MoveDocument(activity.ContentResolver, source.Inner, androidSourceFolder.Inner, Inner);
                if (movedUri is not null)
                {
                    // Rename if needed
                    if (source.Name != newName)
                    {
                        var renamedUri = DocumentsContract.RenameDocument(activity.ContentResolver, movedUri, newName);
                        if (renamedUri is not null)
                            return new AndroidFile(renamedUri, activity, this, permissionRoot);
                    }
                    else
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
