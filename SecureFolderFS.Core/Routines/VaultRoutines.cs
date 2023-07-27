using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.CredentialsRoutines;
using SecureFolderFS.Core.Routines.StorageRoutines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public sealed class VaultRoutines
    {
        private readonly IFolder _vaultFolder;
        private readonly IVaultReader _vaultReader;
        private readonly IVaultWriter _vaultWriter;

        private VaultRoutines(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _vaultReader = new VaultReader(vaultFolder, serializer);
            _vaultWriter = new VaultWriter(vaultFolder, serializer);
        }

        public IUnlockRoutine UnlockVault()
        {
            return new UnlockRoutine(_vaultReader);
        }

        public ICreationRoutine CreateVault()
        {
            return new CreationRoutine(_vaultFolder, _vaultWriter);
        }

        public IStorageRoutine BuildStorage()
        {
            return new StorageRoutine(_vaultFolder);
        }

        public IDisposable SetupAuthentication()
        {
            throw new NotImplementedException();
        }

        public ICredentialsRoutine ChangePassword()
        {
            return new CredentialsRoutine(_vaultWriter);
        }

        public static async Task<VaultRoutines> CreateRoutinesAsync(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            var vaultValidator = new VaultValidator(serializer);
            await vaultValidator.ValidateAsync(vaultFolder, cancellationToken);

            return new VaultRoutines(vaultFolder, serializer);
        }
    }
}
