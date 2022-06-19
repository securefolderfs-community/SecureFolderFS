using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.StoragePool;

namespace SecureFolderFS.WinUI.Storage.StoragePools
{
    /// <inheritdoc cref="IFilePool"/>
    internal sealed class CachingFilePool : IFilePool
    {
        private readonly IFolder _folder;
        private readonly IFileSystemService _fileSystemService;
        private readonly List<IFile> _files;
        private readonly SemaphoreSlim _semaphore;

        public CachingFilePool(IFolder folder, IFileSystemService fileSystemService)
        {
            _folder = folder;
            _fileSystemService = fileSystemService;
            _files = new();
            _semaphore = new(1, 1);
        }

        /// <inheritdoc/>
        public async Task<bool> ClearPoolAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);
                var result = true;

                for (int i = 0; i < _files.Count; i++)
                {
                    var item = _files[i];
                    var deleted = false;
                    var exists = await _fileSystemService.FileExistsAsync(item.Path).ConfigureAwait(false);

                    if (exists)
                    {
                        deleted = await item.DeleteAsync(true).ConfigureAwait(false);
                        result &= deleted;
                    }

                    if (deleted || !exists)
                        _files.RemoveAt(i);
                }

                return result;
            }
            finally
            {
                _ = _semaphore.Release();
            }
        }

        /// <inheritdoc/>
        public async Task<IFile?> RequestFileAsync(string fileName, CancellationToken cancellationToken = default)
        {
            var file = _files.Find(x => x.Name.Equals(fileName));
            if (file is not null)
                return file;

            file = await _folder.CreateFileAsync(fileName, CreationCollisionOption.OpenIfExists).ConfigureAwait(false);
            if (file is not null)
                _files.Add(file);

            return file;
        }
    }
}
