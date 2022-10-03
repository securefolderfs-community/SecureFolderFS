using SecureFolderFS.Core.FileSystem;
using SecureFolderFS.Core.Models;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
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
        Task ReadConfigurationAsync(Stream configStream, IAsyncSerializer<byte[]> serializer, CancellationToken cancellationToken = default);

        Task ReadKeystoreAsync(Stream keystoreStream, IAsyncSerializer<byte[]> serializer, CancellationToken cancellationToken = default);

        void SetContentFolder(IModifiableFolder contentFolder); // TODO: Not so good, refactor later

        void DeriveKeystore(IPassword password);

        // TODO: Change name of this function
        Task<IMountableFileSystem> PrepareAndUnlockAsync(FileSystemOptions fileSystemOptions, CancellationToken cancellationToken = default);
    }
}
