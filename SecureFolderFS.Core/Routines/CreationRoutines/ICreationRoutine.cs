using SecureFolderFS.Core.Models;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.CreationRoutines
{
    // TODO: Needs docs
    public interface ICreationRoutine : IDisposable
    {
        Task CreateContentFolderAsync(IModifiableFolder vaultFolder, CancellationToken cancellationToken = default);

        void SetVaultPassword(IPassword password, CancellationToken cancellationToken = default);

        Task WriteKeystoreAsync(Stream keystoreStream, IAsyncSerializer<byte[]> serializer, CancellationToken cancellationToken = default);

        Task WriteConfigurationAsync(VaultOptions vaultOptions, Stream configStream, IAsyncSerializer<byte[]> serializer, CancellationToken cancellationToken = default);
    }
}
