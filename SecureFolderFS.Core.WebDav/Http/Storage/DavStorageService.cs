using SecureFolderFS.Core.WebDav.Http.Storage.StorageProperties;
using SecureFolderFS.Core.WebDav.Storage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Http.Storage
{
    /// <inheritdoc cref="IStorageService"/>
    internal sealed class DavStorageService : IStorageService
    {
        private readonly ILocatableFolder _baseDirectory;
        private readonly IStorageService _storageService;

        public DavStorageService(ILocatableFolder baseDirectory, IStorageService storageService)
        {
            _baseDirectory = baseDirectory;
            _storageService = storageService;
        }

        /// <inheritdoc/>
        public Task<bool> IsAccessibleAsync(CancellationToken cancellationToken = default)
        {
            return _storageService.IsAccessibleAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var realPath = GetPathFromUriPath(path);
                return await _storageService.FileExistsAsync(realPath, cancellationToken);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var realPath = GetPathFromUriPath(path);
                return await _storageService.DirectoryExistsAsync(realPath, cancellationToken);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task<ILocatableFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUriPath(path);
            var folder = await _storageService.GetFolderFromPathAsync(realPath, cancellationToken);
            var properties = new DavBasicProperties<IDavFolder>();
            var davFolder = new DavFolder(folder, properties);
            properties.Storable = davFolder;

            return davFolder;
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUriPath(path);
            var file = await _storageService.GetFileFromPathAsync(realPath, cancellationToken);
            var properties = new DavBasicProperties<IDavFile>();
            var davFile = new DavFile(file, properties);
            properties.Storable = davFile;

            return davFile;
        }

        private string GetPathFromUriPath(string uriPath)
        {
            var decodedPath = uriPath.Substring(1).Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(_baseDirectory.Path, decodedPath);

            if (fullPath != _baseDirectory.Path && !fullPath.StartsWith(_baseDirectory.Path + Path.DirectorySeparatorChar))
                throw new SecurityException($"The requested path was outside base directory.");

            return fullPath;
        }
    }
}
