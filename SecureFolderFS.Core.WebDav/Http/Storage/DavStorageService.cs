using SecureFolderFS.Core.WebDav.Http.Storage.StorageProperties;
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
                _ = await GetFileFromPathAsync(path, cancellationToken);
                return true;
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
                _ = await GetFolderFromPathAsync(path, cancellationToken);
                return true;
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
            var properties = new DavBasicProperties();

            return new DavFolder(folder, properties);
        }

        /// <inheritdoc/>
        public async Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUriPath(path);
            var file = await _storageService.GetFileFromPathAsync(realPath, cancellationToken);
            var properties = new DavBasicProperties();

            return new DavFile(file, properties);
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
