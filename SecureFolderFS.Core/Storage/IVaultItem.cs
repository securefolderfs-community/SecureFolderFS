using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Paths;

namespace SecureFolderFS.Core.Storage
{
    public interface IVaultItem
    {
        ICiphertextPath CiphertextPath { get; }

        Task DeleteAsync(IProgress<float> progress, CancellationToken cancellationToken);
    }
}
