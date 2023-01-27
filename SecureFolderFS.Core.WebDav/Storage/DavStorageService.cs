using SecureFolderFS.Sdk.Storage;
using NWebDav.Server.Storage;
using SecureFolderFS.Core.WebDav.EncryptingStorage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.IO;
using System.Security;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IStorageService"/>
    internal class DavStorageService : IStorageService, IInstantiableDavStorage
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
                var realPath = GetPathFromUri(path);
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
                var realPath = GetPathFromUri(path);
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
            var realPath = GetPathFromUri(path);
            var folder = await storageService.GetFolderFromPathAsync(realPath, cancellationToken);

            return NewFolder(folder);
        }

        /// <inheritdoc/>
        public virtual async Task<ILocatableFile> GetFileFromPathAsync(string path, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUri(path);
            var file = await storageService.GetFileFromPathAsync(realPath, cancellationToken);

            return NewFile(file);
        }


        /// <inheritdoc/>
        public virtual IDavFile NewFile<T>(T inner) where T : IFile
        {
            return new DavFile<T>(inner);
        }

        /// <inheritdoc/>
        public virtual IDavFolder NewFolder<T>(T inner) where T : IFolder
        {
            return new DavFolder<T>(inner);
        }

        /// <summary>
        /// Converts an <see cref="Uri"/> represented by a <see cref="string"/> into file system friendly path.
        /// </summary>
        /// <param name="uriPath">An <see cref="string"/> in <see cref="Uri"/> format that points to a file/folder.</param>
        /// <returns>If successful, returns a filepath, otherwise false.</returns>
        /// <remarks>
        /// This method does not guarantee existence of the resource that the returned path points to.
        /// </remarks>
        /// <exception cref="SecurityException">Thrown when provided <paramref name="uriPath"/> points to a resource outside the base directory.</exception>
        protected virtual string GetPathFromUri(string uriPath)
        {
            var decodedPath = uriPath.Substring(1).Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(baseDirectory.Path, decodedPath);

            if (fullPath != baseDirectory.Path && !fullPath.StartsWith(baseDirectory.Path + Path.DirectorySeparatorChar))
                throw new SecurityException("The requested path was outside base directory.");

            return fullPath;
        }
    }
}
