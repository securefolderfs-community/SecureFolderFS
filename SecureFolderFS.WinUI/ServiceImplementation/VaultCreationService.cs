using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Routines;
using SecureFolderFS.Core.VaultCreator.Routine;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.Storage.Enums;
using SecureFolderFS.Sdk.Storage.LocatableStorage;
using SecureFolderFS.Sdk.Storage.ModifiableStorage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.WinUI.ServiceImplementation
{
    /// <inheritdoc cref="IVaultCreationService"/>
    internal sealed class VaultCreationService : IVaultCreationService
    {
        private IVaultCreationRoutineStep3? _step3;
        private IModifiableFolder? _folder;

        /// <inheritdoc/>
        public Task<bool> SetVaultFolderAsync(IModifiableFolder folder, CancellationToken cancellationToken = default)
        {
            if (folder is not ILocatableFolder vaultFolder)
                return Task.FromResult(false);

            _step3 = VaultRoutines.NewVaultCreationRoutine()
                .EstablishRoutine()
                .SetVaultFolder(vaultFolder)
                .AddFileOperations();

            return Task.FromResult(true);
        }

        /// <inheritdoc/>
        public Task<bool> SetPasswordAsync(IPassword password, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }

        /// <inheritdoc/>
        public async Task<IFile?> PrepareConfigurationAsync(CancellationToken cancellationToken = default)
        {
            if (_folder is null)
                return null;

            var file = await _folder.CreateFileAsync(Core.Constants.VAULT_CONFIGURATION_FILENAME, CreationCollisionOption.OpenIfExists, cancellationToken);
            if (file is null)
                return null;


            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> PrepareKeystoreAsync(Stream keystoreStream, IAsyncSerializer<Stream> serializer,
            CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> SetContentCipherSchemeAsync(ICipherInfoModel cipherScheme, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<bool> SetFilenameCipherSchemeAsync(ICipherInfoModel cipherScheme, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public Task<IResult> DeployAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
