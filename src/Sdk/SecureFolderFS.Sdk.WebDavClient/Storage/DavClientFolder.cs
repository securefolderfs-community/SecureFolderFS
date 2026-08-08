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
        IRenamableFolder,
        IChildFolder,
        IGetFirstByName,
        IGetItem,
        ICreateRenamedCopyOf,
        IMoveRenamedFrom,
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
            var resolved = await ResolveStorableAsync(path, cancellationToken);
            return resolved ?? throw new FileNotFoundException($"Item with name '{name}' was not found in folder '{Name}'.");
        }

        /// <inheritdoc/>
        public async Task<IStorableChild> GetItemAsync(string id, CancellationToken cancellationToken = default)
        {
            var resolved = await ResolveStorableAsync(id, cancellationToken);
            return resolved ?? throw new FileNotFoundException($"Item with id '{id}' was not found.");
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
        public Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, CancellationToken cancellationToken,
            CreateCopyOfDelegate fallback)
        {
            return CreateCopyOfAsync(fileToCopy, overwrite, fileToCopy.Name, cancellationToken, (mf, f, ov, _, ct) => fallback(mf, f, ov, ct));
        }

        /// <inheritdoc/>
        public async Task<IChildFile> CreateCopyOfAsync(IFile fileToCopy, bool overwrite, string newName,
            CancellationToken cancellationToken, CreateRenamedCopyOfDelegate fallback)
        {
            if (fileToCopy is not DavClientFile davFile)
                return await fallback(this, fileToCopy, overwrite, newName, cancellationToken);

            var destPath = CombinePath(Id, newName);

            // No-op if source and destination are identical
            if (davFile.Id == destPath)
                return davFile;

            var sourceUri = ResolveUri(davFile.Id);
            var destUri = ResolveUri(destPath);
            var copyParams = new CopyParameters()
            {
                Overwrite = overwrite,
                CancellationToken = cancellationToken
            };

            // COPY's destination URI already includes the final name, so the server places the copy
            // directly at destPath — no follow-up rename is needed. (The previous code then issued a
            // MOVE from destPath onto itself — renamedPath == destPath — which WebDAV rejects with 403.)
            var response = await davClient.Copy(sourceUri, destUri, copyParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to copy '{fileToCopy.Name}' to '{newName}': {response.StatusCode}");

            return new DavClientFile(davClient, httpClient, baseUri, destPath, newName, this);
        }

        /// <inheritdoc/>
        public Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite,
            CancellationToken cancellationToken, MoveFromDelegate fallback)
        {
            return MoveFromAsync(fileToMove, source, overwrite, fileToMove.Name, cancellationToken,
                (mf, f, src, ov, _, ct) => fallback(mf, f, src, ov, ct));
        }

        /// <inheritdoc/>
        public async Task<IChildFile> MoveFromAsync(IChildFile fileToMove, IModifiableFolder source, bool overwrite,
            string newName, CancellationToken cancellationToken, MoveRenamedFromDelegate fallback)
        {
            if (fileToMove is not DavClientFile davFile)
                return await fallback(this, fileToMove, source, overwrite, newName, cancellationToken);

            var destPath = CombinePath(Id, newName);

            // No-op if source and destination are identical
            if (davFile.Id == destPath)
                return davFile;

            var sourceUri = ResolveUri(davFile.Id);
            var destUri = ResolveUri(destPath);
            var moveParams = new MoveParameters()
            {
                Overwrite = overwrite,
                CancellationToken = cancellationToken
            };

            var response = await davClient.Move(sourceUri, destUri, moveParams);
            if (!response.IsSuccessful)
                throw new IOException($"Failed to move '{fileToMove.Name}' to '{newName}': {response.StatusCode}");

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

        /// <summary>
        /// Resolves a WebDAV path to a file or folder storable using a single PROPFIND per probe.
        /// </summary>
        /// <remarks>
        /// Two addressing forms exist for the same resource: no trailing slash (file) and trailing
        /// slash (collection). The probe order is platform-dependent:
        /// <list type="bullet">
        /// <item><b>Browser (WASM):</b> file-first. A trailing slash on a file is a path the server
        /// doesn't have, so its CORS preflight returns a non-OK status and
        /// the browser blocks the request. Probing the no-slash form first avoids generating that
        /// failing request for the common case (all vault config files are files).</item>
        /// <item><b>Elsewhere:</b> folder-first, to dodge the server's 301 redirect from
        /// <c>dir</c> to <c>dir/</c>, which makes <see cref="SocketsHttpHandler"/> drop the
        /// Authorization header on the redirected request.</item>
        /// </list>
        /// </remarks>
        private async Task<IStorableChild?> ResolveStorableAsync(string path, CancellationToken cancellationToken)
        {
            var trimmed = path.TrimEnd('/');
            var filePath = trimmed;
            var folderPath = trimmed + "/";
            var name = Uri.UnescapeDataString(trimmed.Split('/').Last(s => !string.IsNullOrEmpty(s)));

            var propfindParams = new PropfindParameters()
            {
                CancellationToken = cancellationToken
            };

            if (OperatingSystem.IsBrowser())
            {
                // File-first
                var asFile = await TryProbeAsync(filePath, propfindParams);
                if (asFile is not null)
                {
                    return asFile == StorableType.Folder
                        ? new DavClientFolder(davClient, httpClient, baseUri, folderPath, name, this)
                        : new DavClientFile(davClient, httpClient, baseUri, filePath, name, this);
                }

                var asFolder = await TryProbeAsync(folderPath, propfindParams);
                if (asFolder is not null)
                    return new DavClientFolder(davClient, httpClient, baseUri, folderPath, name, this);

                return null;
            }

            // Folder-first (non-browser)
            var folderProbe = await TryProbeAsync(folderPath, propfindParams);
            if (folderProbe == StorableType.Folder)
                return new DavClientFolder(davClient, httpClient, baseUri, folderPath, name, this);

            var fileProbe = await TryProbeAsync(filePath, propfindParams);
            if (fileProbe is not null)
            {
                return fileProbe == StorableType.Folder
                    ? new DavClientFolder(davClient, httpClient, baseUri, folderPath, name, this)
                    : new DavClientFile(davClient, httpClient, baseUri, filePath, name, this);
            }

            return null;
        }

        /// <summary>
        /// Performs a Depth:0 PROPFIND on <paramref name="probePath"/>. Returns whether the resolved
        /// resource is a collection or a file, or null when the resource does not exist / the request
        /// failed (including a failed CORS preflight in the browser, which is only ever a negative
        /// signal here).
        /// </summary>
        private async Task<StorableType?> TryProbeAsync(string probePath, PropfindParameters propfindParams)
        {
            try
            {
                var response = await davClient.Propfind(ResolveUri(probePath), propfindParams);
                if (!response.IsSuccessful || !response.Resources.Any())
                    return null;

                // Prefer the resource that matches the probed path; fall back to the first entry
                // (a Depth:0 PROPFIND returns the addressed resource itself).
                var trimmedProbe = probePath.TrimEnd('/');
                var resource = response.Resources.FirstOrDefault(r => (r.Uri ?? string.Empty).TrimEnd('/').EndsWith(trimmedProbe, StringComparison.Ordinal))
                    ?? response.Resources.First();

                return resource.IsCollection ? StorableType.Folder : StorableType.File;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return null;
            }
        }
    }
}
