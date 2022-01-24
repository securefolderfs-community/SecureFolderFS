using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Storage
{
    public interface IVaultFolder : IVaultItem
    {
        Task DeleteAsync(bool recursive, IProgress<float> progress, CancellationToken cancellationToken);
    }
}
