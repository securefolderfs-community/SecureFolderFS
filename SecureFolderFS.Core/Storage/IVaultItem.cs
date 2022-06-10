using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Sdk.Paths;

namespace SecureFolderFS.Core.Storage
{
    [Obsolete("This interface should no longer be used and will be replaced with SecureFolderFS.Sdk.Storage")]
    public interface IVaultItem
    {
        ICiphertextPath CiphertextPath { get; }

        Task DeleteAsync(IProgress<float> progress, CancellationToken cancellationToken);
    }
}
