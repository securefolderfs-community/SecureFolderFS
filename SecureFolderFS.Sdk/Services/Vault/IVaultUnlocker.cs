using SecureFolderFS.Sdk.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Sdk.Services.Vault
{
    public interface IVaultUnlocker
    {
        Task<IVaultLifetimeModel> UnlockAsync(IFolder vaultFolder, IDisposable credentials, IFileSystemInfoModel fileSystem, CancellationToken cancellationToken = default);
    }
}
