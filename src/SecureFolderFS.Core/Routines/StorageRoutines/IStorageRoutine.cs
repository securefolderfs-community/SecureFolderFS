using OwlCore.Storage;
using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.FileSystem.AppModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.StorageRoutines
{
    // TODO: Needs docs
    public interface IStorageRoutine
    {
        IStorageRoutine SetUnlockContract(IDisposable unlockContract);

        Task<IFolder> CreateStorageRootAsync(FileSystemOptions options, CancellationToken cancellationToken);

        Task<IMountableFileSystem> CreateMountableAsync(FileSystemOptions options, CancellationToken cancellationToken);
    }
}
