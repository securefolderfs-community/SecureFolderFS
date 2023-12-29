using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    /// <inheritdoc cref="IStorageService"/>
    public class CryptoStorageService : IStorageService, IWrappable<IFile>, IWrappable<IFolder>
    {
        protected readonly IStorageService storageService;

        public CryptoStorageService(IStorageService storageService)
        {
            this.storageService = storageService;
        }

        /// <inheritdoc/>
        public virtual Task<IFile> GetFileAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual Task<IFolder> GetFolderAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFile> Wrap(IFile file)
        {
            return new CryptoFile(file);
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFolder> Wrap(IFolder folder)
        {
            return new CryptoFolder(folder);
        }
    }
}
