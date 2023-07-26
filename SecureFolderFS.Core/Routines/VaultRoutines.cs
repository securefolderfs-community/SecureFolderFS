using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.PasswordChangeRoutines;
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
        private readonly IAsyncSerializer<Stream> _serializer;
        private readonly IVaultReader _vaultReader;
        private readonly IVaultWriter _vaultWriter;

        private VaultRoutines(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
            _vaultReader = new VaultReader(vaultFolder, serializer);
            _vaultWriter = new VaultWriter(vaultFolder, serializer);
        }

        public IUnlockRoutine UnlockVault()
        {
            return new UnlockRoutine(_vaultFolder, _vaultReader);
        }

        public ICreationRoutine CreateVault()
        {
            return new CreationRoutine(_vaultFolder, _vaultWriter);
        }

        public IDisposable SetupAuthentication()
        {
            throw new NotImplementedException();
        }

        public IPasswordChangeRoutine ChangePassword()
        {
            throw new NotImplementedException();
        }

        public static async Task<VaultRoutines> CreateRoutineAsync(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            var vaultValidator = new VaultValidator(serializer);
            var validationResult = await vaultValidator.ValidateAsync(vaultFolder, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? new InvalidDataException("Vault folder is not valid.");

            return new VaultRoutines(vaultFolder, serializer);
        }
    }
}
