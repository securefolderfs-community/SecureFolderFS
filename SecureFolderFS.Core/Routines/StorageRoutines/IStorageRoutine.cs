using System;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Sdk.Storage;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    // TODO: Needs docs
    public interface IStorageRoutine : IDisposable
    {
        IStorageRoutine SetUnlockFinalizer(IDisposable unlockFinalizer);

        IStorageRoutine SetStorageService(IStorageService storageService);

        Task<IStorageService> CreateStorageAsync(CancellationToken cancellationToken);

        Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken);
    }
}
