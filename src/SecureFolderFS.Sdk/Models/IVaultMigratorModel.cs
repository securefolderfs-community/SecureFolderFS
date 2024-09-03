using System;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Models
{
    public interface IVaultMigratorModel
    {
        IFolder VaultFolder { get; }

        Task<IDisposable> UnlockAsync(object credentials, CancellationToken cancellationToken = default);

        Task<IResult> MigrateAsync(IDisposable unlockContract, ProgressModel progress, CancellationToken cancellationToken = default);
    }
}
