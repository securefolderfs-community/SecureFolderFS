using NWebDav.Server.Storage;
using SecureFolderFS.Core.WebDav.EncryptingStorage;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using System;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.WebDav.Storage
{
    /// <inheritdoc cref="IStorageService"/>
    internal class DavStorageService : IStorageService, IInstantiableDavStorage
    {
        protected readonly ILocatableFolder baseDirectory;
        protected readonly IStorageService storageService;
        protected readonly string? remoteRootDirectory;

        public DavStorageService(ILocatableFolder baseDirectory, IStorageService storageService, string? remoteRootDirectory = null)
        {
            this.baseDirectory = baseDirectory;
            this.storageService = storageService;
            this.remoteRootDirectory = remoteRootDirectory;
        }

        /// <inheritdoc/>
        public virtual async Task<IFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUri(id);
            var folder = await storageService.GetFolderAsync(realPath, cancellationToken);

            return NewFolder(folder);
        }

        /// <inheritdoc/>
        public virtual async Task<IFile> GetFileAsync(string id, CancellationToken cancellationToken = default)
        {
            var realPath = GetPathFromUri(id);
            var file = await storageService.GetFileAsync(realPath, cancellationToken);

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
            var decodedPath = uriPath;
            if (remoteRootDirectory is not null)
                decodedPath = new Regex($"^\\/{remoteRootDirectory}").Replace(uriPath, string.Empty);

            decodedPath = decodedPath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(baseDirectory.Path, decodedPath);

            if (fullPath != baseDirectory.Path && !fullPath.StartsWith(baseDirectory.Path + Path.DirectorySeparatorChar))
                throw new SecurityException("The requested path was outside base directory.");

            return fullPath;
        }
    }
}
