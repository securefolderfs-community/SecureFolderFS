using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.CredentialsRoutines
{
    public interface ICredentialsRoutine : IDisposable
    {
        ICredentialsRoutine SetUnlockFinalizer(IDisposable unlockFinalizer);

        // TODO: The two-way information about the credential method will be provided in a separate routine/helper class
        ICredentialsRoutine SetCredentials(IPassword password, SecretKey? magic);

        // TODO: Would be nice if it also returned IDisposable
        Task FinalizeAsync(CancellationToken cancellationToken);
    }
}
