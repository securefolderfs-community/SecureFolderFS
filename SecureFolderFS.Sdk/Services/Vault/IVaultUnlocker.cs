using SecureFolderFS.Sdk.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services.Vault
{
    public interface IVaultUnlocker
    {
        Task<IVaultLifecycle> UnlockAsync(IVaultModel vaultModel, IDisposable passkey, CancellationToken cancellationToken = default);
    }
}
