using System.Runtime.CompilerServices;
using Android.Provider;
using Android.Webkit;
using AndroidX.DocumentFile.Provider;
using AndroidX.Navigation;
using OwlCore.Storage;
using SecureFolderFS.Maui.Platforms.Android.Storage.StorageProperties;
using SecureFolderFS.Storage.Renamable;
using SecureFolderFS.Storage.StorageProperties;
using Activity = Android.App.Activity;
using AndroidUri = Android.Net.Uri;

namespace SecureFolderFS.Maui.Platforms.Android.Storage
{
    /// <inheritdoc cref="IChildFolder"/>
    internal sealed class AndroidFolder : AndroidStorable, IModifiableFolder, IChildFolder, IGetFirstByName, IRenamableFolder // TODO: Implement: IGetFirstByName, IGetItem
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

                    return Task.FromResult<IStorableChild>(new AndroidFolder(uri, activity, parent, permissionRoot));
                }

                case AndroidFile file:
                {
                    if (activity.ContentResolver is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    var uri = DocumentsContract.RenameDocument(activity.ContentResolver, file.Inner, newName);
                    if (uri is null)
                        return Task.FromException<IStorableChild>(RenameException);

                    return Task.FromResult<IStorableChild>(new AndroidFile(uri, activity, parent, permissionRoot));
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
                throw new FileNotFoundException($"No storage item with the name '{name}' could be found.");

            return target;
        }
        
        /// <inheritdoc/>
        public override Task<IBasicProperties> GetPropertiesAsync()
        {
            if (Document is null)
                return Task.FromException<IBasicProperties>(new ArgumentNullException(nameof(Document)));

            properties ??= new AndroidFileProperties(Document);
            return Task.FromResult(properties);
        }
    }
}
