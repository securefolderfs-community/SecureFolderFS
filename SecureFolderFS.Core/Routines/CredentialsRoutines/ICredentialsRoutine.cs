using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Core.Routines.CredentialsRoutines
{
    public interface ICredentialsRoutine : IDisposable
    {
        ICredentialsRoutine SetUnlockContract(IDisposable unlockContract);

        ICredentialsRoutine SetCredentials(IPassword password, SecretKey? magic);

        // TODO: Would be nice if it also returned IDisposable
        Task FinalizeAsync(CancellationToken cancellationToken);
    }
}
