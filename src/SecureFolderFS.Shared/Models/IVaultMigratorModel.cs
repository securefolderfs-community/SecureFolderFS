using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Models
{
    public interface IVaultMigratorModel
    {
        IFolder VaultFolder { get; }

        Task<IDisposable> UnlockAsync<T>(T credentials, CancellationToken cancellationToken = default);

        Task MigrateAsync(IDisposable unlockContract, ProgressModel<IResult> progress, CancellationToken cancellationToken = default);
    }
}
