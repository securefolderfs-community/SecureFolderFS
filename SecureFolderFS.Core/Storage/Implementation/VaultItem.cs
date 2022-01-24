using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Core.Paths;

namespace SecureFolderFS.Core.Storage.Implementation
{
    internal abstract class VaultItem : IVaultItem
    {
        protected readonly IFileSystemOperations fileSystemOperations;

        public ICiphertextPath CiphertextPath { get; }

        protected VaultItem(ICiphertextPath ciphertextPath, IFileSystemOperations fileSystemOperations)
        {
            this.CiphertextPath = ciphertextPath;
            this.fileSystemOperations = fileSystemOperations;
        }

        public abstract Task DeleteAsync(IProgress<float> progress, CancellationToken cancellationToken);
    }
}
