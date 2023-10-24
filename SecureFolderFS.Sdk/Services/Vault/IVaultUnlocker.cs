using SecureFolderFS.Sdk.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Services.Vault
{
    public interface IVaultUnlocker
    {
        Task<IVaultLifetimeModel> UnlockAsync(IVaultModel vaultModel, IDisposable credentials, CancellationToken cancellationToken = default);
    }
}
