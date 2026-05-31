using OwlCore.Storage;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Core.VaultAccess;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines.Operational
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
            // In the case of creation the validation is not triggered
            return new CreationRoutine(_vaultFolder, VaultWriter);
        }

        public AppPlatformCreationRoutine CreateAppPlatformVault()
        {
            return new AppPlatformCreationRoutine(_vaultFolder, VaultWriter);
        }

        public ICredentialsRoutine UnlockVault()
        {
            CheckVaultValidation();
            return new UnlockRoutine(VaultReader);
        }

        public ICredentialsRoutine UnlockAppPlatformVault()
        {
            CheckVaultValidation();
            return new AppPlatformUnlockRoutine(VaultReader);
        }

        public ICredentialsRoutine RecoverVault()
        {
            CheckVaultValidation();
            return new RecoverRoutine(VaultReader);
        }

        public ICredentialsRoutine RestoreVault()
        {
            // In the case of restoring the validation is not triggered since the vault is expected to be in an invalid state
            return new RestoreRoutine(_vaultFolder, VaultWriter);
        }

        public IModifyCredentialsRoutine ModifyCredentials()
        {
            CheckVaultValidation();
            return new ModifyCredentialsRoutine(VaultReader, VaultWriter);
        }

        public ModifyComplementationRoutine ModifyComplementation()
        {
            CheckVaultValidation();
            return new ModifyComplementationRoutine(VaultReader, VaultWriter);
        }

        private void CheckVaultValidation()
        {
            if (!_validationResult.Successful)
                throw _validationResult.Exception ?? new InvalidDataException("Vault format is invalid.");
        }

        public static async Task<VaultRoutines> CreateRoutinesAsync(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            var vaultValidator = new VaultValidator(serializer);
            var validationResult = await vaultValidator.TryValidateAsync(vaultFolder, cancellationToken);

            return new VaultRoutines(vaultFolder, serializer, validationResult);
        }
    }
}
