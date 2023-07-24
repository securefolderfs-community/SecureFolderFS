using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    // TODO: Needs docs
    public interface ICreationRoutine : IDisposable
    {
        ICreationRoutine SetPassword(IPassword password);

        ICreationRoutine SetOptions(VaultOptions vaultOptions);

        Task FinalizeAsync(CancellationToken cancellationToken);
    }

    public interface IAuthenticationCreationRoutine : IDisposable
    {
        IAsyncInitialize AsWindowsHello();

        IAsyncInitialize AsHardwareKey();

        IAsyncInitialize AsKeyFile();
    }
}
