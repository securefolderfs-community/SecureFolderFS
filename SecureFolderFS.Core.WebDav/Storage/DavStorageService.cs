using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IStorageService"/>
    internal class DavStorageService : IStorageService
    {
        protected readonly ILocatableFolder baseDirectory;
        protected readonly IStorageService storageService;

        public DavStorageService(ILocatableFolder baseDirectory, IStorageService storageService)
        {
            this.baseDirectory = baseDirectory;
            this.storageService = storageService;
        }

        /// <inheritdoc/>
        public virtual Task<bool> IsAccessibleAsync(CancellationToken cancellationToken = default)
        {
            return storageService.IsAccessibleAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public virtual async Task<bool> FileExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var realPath = GetPathFromUriPath(path);
                return await storageService.FileExistsAsync(realPath, cancellationToken);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<bool> DirectoryExistsAsync(string path, CancellationToken cancellationToken = default)
        {
            try
            {
                var realPath = GetPathFromUriPath(path);
                return await storageService.DirectoryExistsAsync(realPath, cancellationToken);
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public virtual async Task<ILocatableFolder> GetFolderFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUriPath(path);
            var folder = await storageService.GetFolderFromPathAsync(realPath, cancellationToken);
            var davFolder = new DavFolder<ILocatableFolder>(folder);

            return davFolder;
        }

        /// <inheritdoc/>
        public virtual async Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUriPath(path);
            var file = await storageService.GetFileFromPathAsync(realPath, cancellationToken);
            var davFile = new DavFile<ILocatableFile>(file);

            return davFile;
        }

        private string GetPathFromUriPath(string uriPath)
        {
            var decodedPath = uriPath.Substring(1).Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(baseDirectory.Path, decodedPath);

            if (fullPath != baseDirectory.Path && !fullPath.StartsWith(baseDirectory.Path + Path.DirectorySeparatorChar))
                throw new SecurityException($"The requested path was outside base directory.");

            return fullPath;
        }
    }
}
