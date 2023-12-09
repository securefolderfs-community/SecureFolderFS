using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.CredentialsRoutines;
using SecureFolderFS.Core.Routines.StorageRoutines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines
{
    // TODO: Needs docs
    public sealed class VaultRoutines
    {
        private readonly IFolder _vaultFolder;
        private readonly IResult _validationResult;
        private readonly VaultReader _vaultReader;
        private readonly VaultWriter _vaultWriter;

        private VaultRoutines(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, IResult validationResult)
        {
            _vaultFolder = vaultFolder;
            _vaultReader = new VaultReader(vaultFolder, serializer);
            _vaultWriter = new VaultWriter(vaultFolder, serializer);
            _validationResult = validationResult;
        }

        public ICreationRoutine CreateVault()
        {
            // Only in the case of creation, the validity is not checked
            return new CreationRoutine(_vaultFolder, _vaultWriter);
        }

        public IUnlockRoutine UnlockVault()
        {
            CheckVaultValidation();
            return new UnlockRoutine(_vaultReader);
        }

        public IStorageRoutine BuildStorage()
        {
            CheckVaultValidation();
            return new StorageRoutine(_vaultFolder);
        }

        public IDisposable SetupAuthentication()
        {
            CheckVaultValidation();
            throw new NotImplementedException();
        }

        public ICredentialsRoutine ChangePassword()
        {
            CheckVaultValidation();
            return new CredentialsRoutine(_vaultWriter);
        }

        private void CheckVaultValidation()
        {
            if (!_validationResult.Successful)
                throw _validationResult.Exception ?? new InvalidDataException("Vault is not valid");
        }

        public static async Task<VaultRoutines> CreateRoutinesAsync(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            var vaultValidator = new VaultValidator(serializer);
            var validationResult = await vaultValidator.TryValidateAsync(vaultFolder, cancellationToken);

            return new VaultRoutines(vaultFolder, serializer, validationResult);
        }
    }
}
