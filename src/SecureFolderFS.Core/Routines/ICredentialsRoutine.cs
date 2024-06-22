using SecureFolderFS.Core.Cryptography.SecureStore;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public interface ICredentialsRoutine : IAsyncInitialize, IDisposable
    {
        void SetCredentials(SecretKey passkey);

        Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken);
    }
}
