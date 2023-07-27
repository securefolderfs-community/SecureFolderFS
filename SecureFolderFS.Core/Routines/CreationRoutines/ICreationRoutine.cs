using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    // TODO: Needs docs
    public interface ICreationRoutine : IDisposable
    {
        ICreationRoutine SetCredentials(IPassword password, SecretKey? magic);

        ICreationRoutine SetOptions(VaultOptions vaultOptions);

        Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken);
    }
}
