using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Storage
{
    [Obsolete("This interface should no longer be used and will be replaced with SecureFolderFS.Sdk.Storage.")]
    public interface IVaultFolder : IVaultItem
    {
        Task DeleteAsync(bool recursive, IProgress<float> progress, CancellationToken cancellationToken);
    }
}
