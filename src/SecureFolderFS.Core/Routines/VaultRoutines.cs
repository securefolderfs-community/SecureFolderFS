using OwlCore.Storage;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.CredentialsRoutines;
using SecureFolderFS.Core.Routines.StorageRoutines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
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

        public VaultReader VaultReader { get; }

        public VaultWriter VaultWriter { get; }

        private VaultRoutines(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, IResult validationResult)
        {
            _vaultFolder = vaultFolder;
            VaultReader = new VaultReader(vaultFolder, serializer);
            VaultWriter = new VaultWriter(vaultFolder, serializer);
            _validationResult = validationResult;
        }

        public ICreationRoutine CreateVault()
        {
            // Only in the case of creation the validation is not triggered
            return new CreationRoutine(_vaultFolder, VaultWriter);
        }

        public IUnlockRoutine UnlockVault()
        {
            CheckVaultValidation();
            return new UnlockRoutine(VaultReader);
        }

        public IStorageRoutine BuildStorage()
        {
            CheckVaultValidation();
            return new StorageRoutine(_vaultFolder);
        }

        public ICredentialsRoutine ChangeCredentials()
        {
            CheckVaultValidation();
            return new CredentialsRoutine(VaultWriter);
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
