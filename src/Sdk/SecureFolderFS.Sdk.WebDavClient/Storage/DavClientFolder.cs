using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.WebDavClient.Storage.StorageProperties;
using SecureFolderFS.Storage.Renamable;
using WebDav;

namespace SecureFolderFS.Sdk.WebDavClient.Storage
{
    public class DavClientFolder : DavClientStorable,
        IChildFolder,
        IRenamableFolder,
        IGetFirstByName,
        IGetItem,
        ICreatedAt,
        ILastModifiedAt
    {
        /// <inheritdoc/>
        public ICreatedAtProperty CreatedAt => field ??= new DavClientCreatedAtProperty(Id, client, baseUri);

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new DavClientLastModifiedAtProperty(Id, client, baseUri);

        public DavClientFolder(IWebDavClient client, Uri baseUri, string id, string name, IFolder? parentFolder = null)
            : base(client, baseUri, id, name, parentFolder)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var folderUri = ResolveUri(Id.EndsWith('/') ? Id : Id + "/");
            var propfindParams = new PropfindParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.Propfind(folderUri, propfindParams);

            if (!response.IsSuccessful)
                throw new IOException($"Failed to list items in folder '{Name}': {response.StatusCode}");

            foreach (var resource in response.Resources)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var resourcePath = resource.Uri ?? string.Empty;

                // Skip the folder itself
                var normalizedResource = resourcePath.TrimEnd('/');
                if (normalizedResource == Id.TrimEnd('/'))
                    continue;

                var name = Uri.UnescapeDataString(normalizedResource.Split('/').Last(s => !string.IsNullOrEmpty(s)));
                var isCollection = resource.IsCollection;
                var id = normalizedResource;

                if (isCollection)
                {
                    if (type is StorableType.All or StorableType.Folder)
                        yield return new DavClientFolder(client, baseUri, id, name, this);
                }
                else
                {
                    if (type is StorableType.All or StorableType.File)
                        yield return new DavClientFile(client, baseUri, id, name, this);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var path = CombinePath(Id, name);
            var uri = ResolveUri(path);
            var propfindParams = new PropfindParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.Propfind(uri, propfindParams);

            if (!response.IsSuccessful || !response.Resources.Any())
                throw new FileNotFoundException($"Item with name '{name}' was not found in folder '{Name}'.");

            var resource = response.Resources.First();
            if (resource.IsCollection)
                return new DavClientFolder(client, baseUri, path, name, this);
            else
                return new DavClientFile(client, baseUri, path, name, this);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            var uri = ResolveUri(id);
            var propfindParams = new PropfindParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.Propfind(uri, propfindParams);

            if (!response.IsSuccessful || !response.Resources.Any())
                throw new FileNotFoundException($"Item with id '{id}' was not found.");

            var resource = response.Resources.First();
            var name = Uri.UnescapeDataString(id.TrimEnd('/').Split('/').Last(s => !string.IsNullOrEmpty(s)));

            if (resource.IsCollection)
                return new DavClientFolder(client, baseUri, id, name, this);
            else
                return new DavClientFile(client, baseUri, id, name, this);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            var parent = await storable.GetParentAsync(cancellationToken);
            if (parent is null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{storable.Name}' does not belong to folder '{Name}'.");

            var destPath = CombinePath(Id, newName);
            var sourceUri = ResolveUri(storable.Id);
            var destUri = ResolveUri(destPath);

            var moveParams = new MoveParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.Move(sourceUri, destUri, moveParams);

            if (!response.IsSuccessful)
                throw new IOException($"Failed to rename '{storable.Name}' to '{newName}': {response.StatusCode}");

            if (storable is IFolder)
                return new DavClientFolder(client, baseUri, destPath, newName, this);
            else
                return new DavClientFile(client, baseUri, destPath, newName, this);
        }

        /// <inheritdoc/>
        public Task<IFolderWatcher> GetFolderWatcherAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromException<IFolderWatcher>(new NotSupportedException());
        }

        /// <inheritdoc/>
        public async Task DeleteAsync(IStorableChild item, CancellationToken cancellationToken = default)
        {
            var uri = ResolveUri(item.Id);
            var deleteParams = new DeleteParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.Delete(uri, deleteParams);

            if (!response.IsSuccessful)
                throw new IOException($"Failed to delete '{item.Name}': {response.StatusCode}");
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var id = CombinePath(Id, name);
            var uri = ResolveUri(id);
            var mkcolParams = new MkColParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.Mkcol(uri, mkcolParams);

            if (!response.IsSuccessful)
                throw new IOException($"Failed to create folder '{name}': {response.StatusCode}");

            return new DavClientFolder(client, baseUri, id, name, this);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var id = CombinePath(Id, name);
            var uri = ResolveUri(id);

            using var emptyStream = new MemoryStream();
            var putParams = new PutFileParameters
            {
                CancellationToken = cancellationToken
            };
            var response = await client.PutFile(uri, emptyStream, putParams);

            if (!response.IsSuccessful)
                throw new IOException($"Failed to create file '{name}': {response.StatusCode}");

            return new DavClientFile(client, baseUri, id, name, this);
        }
    }
}
