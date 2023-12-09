using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.CredentialsRoutines
{
    public interface ICredentialsRoutine : IDisposable
    {
        ICredentialsRoutine SetUnlockContract(IDisposable unlockContract);

        ICredentialsRoutine SetCredentials(SecretKey passkey);

        // TODO: Would be nice if it also returned IDisposable
        Task FinalizeAsync(CancellationToken cancellationToken);
    }
}
