using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
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
        public ICreatedAtProperty CreatedAt => field ??= new DavClientCreatedAtProperty(Id, davClient, baseUri);

        /// <inheritdoc/>
        public ILastModifiedAtProperty LastModifiedAt => field ??= new DavClientLastModifiedAtProperty(Id, davClient, baseUri);

        public DavClientFolder(IWebDavClient davClient, HttpClient httpClient, Uri baseUri, string id, string name, IFolder? parentFolder = null)
            : base(davClient, httpClient, baseUri, id, name, parentFolder)
        {
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var folderUri = ResolveUri(Id.EndsWith('/') ? Id : Id + "/");
            var propfindParams = new PropfindParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.Propfind(folderUri, propfindParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to list items in folder '{Name}': {response.StatusCode}");

            foreach (var resource in response.Resources)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var resourcePath = resource.Uri ?? string.Empty;
                var normalizedResource = resourcePath.TrimEnd('/');

                // Skip the folder itself
                if (normalizedResource == Id.TrimEnd('/'))
                    continue;

                var name = Uri.UnescapeDataString(normalizedResource.Split('/').Last(s => !string.IsNullOrEmpty(s)));
                var isCollection = resource.IsCollection;

                if (isCollection)
                {
                    if (type is StorableType.All or StorableType.Folder)
                    {
                        // Restore trailing slash for folders so CombinePath and ResolveUri work correctly downstream
                        var id = normalizedResource + "/";
                        yield return new DavClientFolder(davClient, httpClient, baseUri, id, name, this);
                    }
                }
                else
                {
                    if (type is StorableType.All or StorableType.File)
                        yield return new DavClientFile(davClient, httpClient, baseUri, normalizedResource, name, this);
                }
            }
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetFirstByNameAsync(string name, CancellationToken cancellationToken = default)
        {
            var path = CombinePath(Id, name);

            // Try as a collection first (with trailing slash) to avoid 301 redirect
            // which causes SocketsHttpHandler to strip the Authorization header
            var folderPath = path.EndsWith('/') ? path : path + "/";
            var folderUri = ResolveUri(folderPath);
            var propfindParams = new PropfindParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.Propfind(folderUri, propfindParams);
            if (response.IsSuccessful && response.Resources.Any())
            {
                var resource = response.Resources.First();
                if (resource.IsCollection)
                    return new DavClientFolder(davClient, httpClient, baseUri, folderPath, name, this);
            }

            // Fall back to file (no trailing slash)
            var fileUri = ResolveUri(path);
            response = await davClient.Propfind(fileUri, propfindParams);
            if (!response.IsSuccessful || !response.Resources.Any())
                throw new FileNotFoundException($"Item with name '{name}' was not found in folder '{Name}'.");

            return new DavClientFile(davClient, httpClient, baseUri, path, name, this);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            // Try as collection first (trailing slash) to avoid 301 redirect stripping Authorization header
            var folderPath = id.EndsWith('/') ? id : id + "/";
            var folderUri = ResolveUri(folderPath);
            var propfindParams = new PropfindParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.Propfind(folderUri, propfindParams);
            if (response.IsSuccessful && response.Resources.Any())
            {
                var resource = response.Resources.First();
                if (resource.IsCollection)
                {
                    var name = Uri.UnescapeDataString(folderPath.TrimEnd('/').Split('/').Last(s => !string.IsNullOrEmpty(s)));
                    return new DavClientFolder(davClient, httpClient, baseUri, folderPath, name, this);
                }
            }

            // Fall back to file
            var fileUri = ResolveUri(id);
            response = await davClient.Propfind(fileUri, propfindParams);
            if (!response.IsSuccessful || !response.Resources.Any())
                throw new FileNotFoundException($"Item with id '{id}' was not found.");

            var fileName = Uri.UnescapeDataString(id.TrimEnd('/').Split('/').Last(s => !string.IsNullOrEmpty(s)));
            return new DavClientFile(davClient, httpClient, baseUri, id, fileName, this);
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> RenameAsync(IStorableChild storable, string newName, CancellationToken cancellationToken = default)
        {
            var parent = await storable.GetParentAsync(cancellationToken);
            if (parent is null || parent.Id != Id)
                throw new InvalidOperationException($"Item '{storable.Name}' does not belong to folder '{Name}'.");

            // Ensure source URI has trailing slash for folders to avoid redirect
            var sourceId = storable is IFolder && !storable.Id.EndsWith('/')
                ? storable.Id + "/"
                : storable.Id;
            var sourceUri = ResolveUri(sourceId);
            var destPath = CombinePath(Id, newName);

            // Ensure dest URI has trailing slash for folders
            var destId = storable is IFolder && !destPath.EndsWith('/')
                ? destPath + "/"
                : destPath;

            var destUri = ResolveUri(destId);
            var moveParams = new MoveParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.Move(sourceUri, destUri, moveParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to rename '{storable.Name}' to '{newName}': {response.StatusCode}");

            if (storable is IFolder)
                return new DavClientFolder(davClient, httpClient, baseUri, destId, newName, this);
            else
                return new DavClientFile(davClient, httpClient, baseUri, destPath, newName, this);
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
            var deleteParams = new DeleteParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.Delete(uri, deleteParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to delete '{item.Name}': {response.StatusCode}");
        }

        /// <inheritdoc/>
        public async Task<IChildFolder> CreateFolderAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var id = CombinePath(Id, name);
            var uri = ResolveUri(id.EndsWith('/') ? id : id + "/");
            var mkcolParams = new MkColParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.Mkcol(uri, mkcolParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to create folder '{name}': {response.StatusCode}");

            return new DavClientFolder(davClient, httpClient, baseUri, id, name, this);
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateFileAsync(string name, bool overwrite = false, CancellationToken cancellationToken = default)
        {
            var id = CombinePath(Id, name);
            var uri = ResolveUri(id);

            using var emptyStream = new MemoryStream();
            var putParams = new PutFileParameters()
            {
                CancellationToken = cancellationToken
            };

            var response = await davClient.PutFile(uri, emptyStream, putParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to create file '{name}': {response.StatusCode}");

            return new DavClientFile(davClient, httpClient, baseUri, id, name, this);
        }
    }
}
