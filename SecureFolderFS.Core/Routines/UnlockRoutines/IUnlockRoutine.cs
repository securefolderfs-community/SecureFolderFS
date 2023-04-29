using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.UnlockRoutines
{
    // TODO: Needs docs
    public interface IUnlockRoutine : IDisposable
    {
        string? ContentCipherId { get; }

        string? FileNameCipherId { get; }

        Task SetVaultStoreAsync(IFolder vaultFolder, IStorageService storageService, CancellationToken cancellationToken = default);

        Task ReadConfigurationAsync(Stream configStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default);

        Task ReadKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default);

        void DeriveKeystore(IPassword password);

        // TODO: Change name of this function
        Task<IMountableFileSystem> PrepareAndUnlockAsync(FileSystemOptions fileSystemOptions, CancellationToken cancellationToken = default);
    }
}
