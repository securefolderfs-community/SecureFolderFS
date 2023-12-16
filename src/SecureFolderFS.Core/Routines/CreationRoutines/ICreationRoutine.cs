using SecureFolderFS.Core.Cryptography.SecureStore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    // TODO: Needs docs
    public interface ICreationRoutine : IDisposable
    {
        ICreationRoutine SetCredentials(SecretKey passkey);

        ICreationRoutine SetOptions(IDictionary<string, string?> options);

        Task<IDisposable> FinalizeAsync(CancellationToken cancellationToken);
    }
}
