using OwlCore.Storage;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Shared.Models
{
    // TODO: Needs docs
    public interface IVaultMigratorModel : IDisposable
    {
        IFolder VaultFolder { get; }

        Task<IDisposable> UnlockAsync<T>(T credentials, CancellationToken cancellationToken = default);

        Task MigrateAsync(IDisposable unlockContract, ProgressModel<IResult> progress, CancellationToken cancellationToken = default);
    }
}
