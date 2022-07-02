using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.WinUI.Storage.NativeStorage;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IFileSystemService"/>
    internal sealed class NativeFileSystemService : IFileSystemService
    {
        /// <inheritdoc/>
        public Task<bool> IsFileSystemAccessible()
        {
            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> FileExistsAsync(string path)
        {
            if (File.Exists(path))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<bool> DirectoryExistsAsync(string path)
        {
            if (Directory.Exists(path))
                return Task.FromResult(true);

            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public Task<IFolder?> GetFolderFromPathAsync(string path)
        {
            if (Directory.Exists(path))
                return Task.FromResult<IFolder?>(new NativeFolder(path));

            return Task.FromResult<IFolder?>(null);
        }

        /// <inheritdoc/>
        public Task<IFile?> GetFileFromPathAsync(string path)
        {
            if (File.Exists(path))
                return Task.FromResult<IFile?>(new NativeFile(path));

            return Task.FromResult<IFile?>(null);
        }

        /// <inheritdoc/>
        public Task<IDisposable?> LockFolderAsync(IFolder folder)
        {
            return Task.FromResult<IDisposable?>(null); // TODO: Implement
        }

        /// <inheritdoc/>
        public async Task<TSource?> CopyAsync<TSource>(TSource source, IFolder destinationFolder, NameCollisionOption options, IProgress<double>? progress = null,
            CancellationToken cancellationToken = default) where TSource : IBaseStorage
        {
            try
            {
                var destinationPath = Path.Combine(destinationFolder.Path, source.Name);
                if (source is IFile sourceFile)
                {
                    return (TSource)(IBaseStorage)await Task.Run(() =>
                    {
                        File.Copy(sourceFile.Path, destinationPath, options == NameCollisionOption.ReplaceExisting);
                        return new NativeFile(destinationPath);
                    }, cancellationToken);
                }
                else if (source is IFolder)
                {
                    throw new NotSupportedException();
                }

                throw new ArgumentException("The source type is invalid.", nameof(TSource));
            }
            catch (Exception)
            {
                return default;
            }
        }

        /// <inheritdoc/>
        public async Task<TSource?> MoveAsync<TSource>(TSource source, IFolder destinationFolder, NameCollisionOption options, IProgress<double>? progress = null,
            CancellationToken cancellationToken = default) where TSource : IBaseStorage
        {
            try
            {
                var destinationPath = Path.Combine(destinationFolder.Path, source.Name);
                if (source is IFile sourceFile)
                {
                    return (TSource)(IBaseStorage)await Task.Run(() =>
                    {
                        File.Move(sourceFile.Path, destinationPath, options == NameCollisionOption.ReplaceExisting);
                        return new NativeFile(destinationPath);
                    }, cancellationToken);
                }
                else if (source is IFolder)
                {
                    throw new NotSupportedException();
                }

                throw new ArgumentException("The source type is invalid.", nameof(TSource));
            }
            catch (Exception)
            {
                return default;
            }
        }
    }
}
