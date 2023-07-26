using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    // TODO: Needs docs
    public interface ICreationRoutine : IDisposable
    {
        ICreationRoutine SetPassword(IPassword password);

        ICreationRoutine SetOptions(VaultOptions vaultOptions);

        Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken);
    }
}
