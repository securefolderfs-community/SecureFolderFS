using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Cryptography.Storage
{
    /// <inheritdoc cref="IGetItemRecursive"/>
    public class CryptoStorageService : IGetItemRecursive, IWrappable<IFile>, IWrappable<IFolder>
    {
        protected readonly IGetItemRecursive storageRoot;

        public CryptoStorageService(IGetItemRecursive storageRoot)
        {
            this.storageRoot = storageRoot;
        }

        public string Id => throw new NotImplementedException();

        public string Name => throw new NotImplementedException();


        /// <inheritdoc/>
        public Task<IStorableChild> GetItemRecursiveAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<IStorableChild> GetItemsAsync(StorableType type = StorableType.All, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await Task.CompletedTask;
            yield break;
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFile> Wrap(IFile file)
        {
            return null;
            //return new CryptoFile(file);
        }

        /// <inheritdoc/>
        public virtual IWrapper<IFolder> Wrap(IFolder folder)
        {
            return null;
            //return new CryptoFolder(folder);
        }
    }
}
