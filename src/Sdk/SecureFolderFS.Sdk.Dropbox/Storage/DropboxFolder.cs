using System.Runtime.CompilerServices;
using Dropbox.Api;
using Dropbox.Api.Files;
using OwlCore.Storage;
using SecureFolderFS.Storage.Renamable;

namespace SecureFolderFS.Sdk.Dropbox.Storage
{
    public class DropboxFolder : DropboxStorable, IRenamableFolder, IChildFolder, IGetFirstByName, IGetItem
    {
        public DropboxFolder(DropboxClient client, string id, string name, IFolder? parent = null)
            : base(client, id, name, parent)
        {
        }

        /// <inheritdoc/>
        public virtual async IAsyncEnumerable<IStorableChild> GetItemsAsync(
            StorableType type = StorableType.All,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var result = await Client.Files.ListFolderAsync(Id);

            while (true)
            {
                foreach (var entry in result.Entries)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    if (entry.IsFolder && type is StorableType.All or StorableType.Folder)
                        yield return new DropboxFolder(Client, CombinePaths(Id, entry.Name), entry.Name, this);
                    else if (entry.IsFile && type is StorableType.All or StorableType.File)
                        yield return new DropboxFile(Client, CombinePaths(Id, entry.Name), entry.Name, this);
                }

                if (!result.HasMore)
                    break;

                result = await Client.Files.ListFolderContinueAsync(result.Cursor);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var path = CombinePaths(Id, name);

            try
            {
                var metadata = await Client.Files.GetMetadataAsync(path);

                if (metadata.IsFolder)
                    return new DropboxFolder(Client, path, metadata.Name, this);
                else
                    return new DropboxFile(Client, path, metadata.Name, this);
            }
            catch (ApiException<GetMetadataError> ex) when (ex.ErrorResponse.IsPath)
            {
                throw new FileNotFoundException($"Item with name '{name}' was not found in folder '{Name}'.");
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            // id is a full Dropbox path like "/Photos/vacation.jpg"
            // Verify this item lives within our subtree
            if (!id.StartsWith(Id == string.Empty ? "/" : Id + "/"))
                throw new FileNotFoundException($"Item with id '{id}' is not a child of folder '{Name}'.");

            // Strip our own path prefix to get the relative portion, e.g. "Photos/vacation.jpg"
            var relative = id.Substring(Id.Length + 1);
            var slashIndex = relative.IndexOf('/');
            var directChildName = slashIndex >= 0 ? relative.Substring(0, slashIndex) : relative;
            var directChildPath = CombinePaths(Id, directChildName);

            Metadata metadata;
            try
            {
                metadata = await Client.Files.GetMetadataAsync(directChildPath);
            }
            catch (ApiException<GetMetadataError> ex) when (ex.ErrorResponse.IsPath)
            {
                throw new FileNotFoundException($"Item with id '{id}' was not found in folder '{Name}'.");
            }

            if (metadata.IsFolder)
            {
                var folder = new DropboxFolder(Client, directChildPath, metadata.Name, this);

                // Recurse if the target is deeper than the direct child
                if (id != directChildPath)
                    return await folder.GetItemAsync(id, cancellationToken);

                return folder;
            }
            else
            {
                // Files have no children — the id must match exactly
                if (id != directChildPath)
                    throw new FileNotFoundException($"Item with id '{id}' was not found.");

                return new DropboxFile(Client, directChildPath, metadata.Name, this);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            var parent = await storable.GetParentAsync(cancellationToken);
            if (parent is null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{storable.Name}' does not belong to folder '{Name}'.");

            var targetPath = CombinePaths(Id, newName);

            // Verify the target name doesn't already exist
            try
            {
                await Client.Files.GetMetadataAsync(targetPath);
                throw new IOException($"An item with name '{newName}' already exists in folder '{Name}'.");
            }
            catch (ApiException<GetMetadataError> ex) when (ex.ErrorResponse.IsPath)
            {
                // Path not found — safe to rename
            }

            var result = await Client.Files.MoveV2Async(storable.Id, targetPath, autorename: false);
            var moved = result.Metadata;

            if (moved.IsFolder)
                return new DropboxFolder(Client, targetPath, moved.Name, this);
            else
                return new DropboxFile(Client, targetPath, moved.Name, this);
        }

        /// <inheritdoc/>
        public virtual Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
            => Task.FromException<IFolderWatcher>(new NotImplementedException());

        /// <inheritdoc/>
        public virtual async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            if (item is not DropboxStorable)
                throw new ArgumentException($"Item '{item.Name}' is not a Dropbox item.", nameof(item));

            var parent = await item.GetParentAsync(cancellationToken);
            if (parent is null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{item.Name}' does not belong to folder '{Name}'.");

            await Client.Files.DeleteV2Async(item.Id);
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var path = CombinePaths(Id, name);

            try
            {
                var meta = await Client.Files.CreateFolderV2Async(path, autorename: false);
                return new DropboxFolder(Client, meta.Metadata.PathDisplay, meta.Metadata.Name, this);
            }
            catch (ApiException<CreateFolderError> ex) when (ex.ErrorResponse.IsPath)
            {
                if (!overwrite)
                    throw new IOException($"Folder '{name}' already exists in '{Name}'.");

                return new DropboxFolder(Client, path, name, this);
            }
        }

        /// <inheritdoc/>
        public virtual async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var path = CombinePaths(Id, name);
            try
            {
                var meta = await Client.Files.UploadAsync(
                    path,
                    overwrite ? WriteMode.Overwrite.Instance : WriteMode.Add.Instance,
                    body: new MemoryStream());
                return new DropboxFile(Client, meta.PathDisplay, meta.Name, this);
            }
            catch (ApiException<UploadError>)
            {
                if (!overwrite)
                    throw new IOException($"File '{name}' already exists in folder '{Name}'.");

                throw;
            }
        }
    }
}