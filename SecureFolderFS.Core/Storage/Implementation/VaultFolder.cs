using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem.Operations;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Storage.Implementation
{
    internal sealed class VaultFolder : VaultItem, IVaultFolder
    {
        public VaultFolder(ICiphertextPath ciphertextPath, IFileSystemOperations fileSystemOperations)
            : base(ciphertextPath, fileSystemOperations)
        {
        }

        public override Task DeleteAsync(IProgress<float> progress, CancellationToken cancellationToken)
        {
            return DeleteAsync(false, progress, cancellationToken);
        }

        public Task DeleteAsync(bool recursive, IProgress<float> progress, CancellationToken cancellationToken)
        {
            //fileSystemOperations.DeleteDirectory(CiphertextPath); // TODO

            return Task.CompletedTask;
        }
    }
}
