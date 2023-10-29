using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Core.Routines.UnlockRoutines
{
    // TODO: Needs docs
    public interface IUnlockRoutine : IAsyncInitialize, IDisposable
    {
        IUnlockRoutine SetCredentials(SecretKey passkey);

        Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken);
    }
}
