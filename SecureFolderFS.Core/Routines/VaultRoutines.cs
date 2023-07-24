using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Core.Routines.CreationRoutines;
using SecureFolderFS.Core.Routines.PasswordChangeRoutines;
using SecureFolderFS.Core.Routines.UnlockRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Core.Routines
{
    public sealed class VaultRoutines
    {
        private readonly IFolder _vaultFolder;
        private readonly IAsyncSerializer<Stream> _serializer;

        private VaultRoutines(IFolder vaultFolder, IAsyncSerializer<Stream> serializer)
        {
            _vaultFolder = vaultFolder;
            _serializer = serializer;
        }

        public IUnlockRoutine UnlockVault()
        {
            return new UnlockRoutine();
        }

        public ICreationRoutine CreateVault()
        {
            return new CreationRoutine();
        }

        public IPasswordChangeRoutine ChangePassword()
        {
            throw new NotImplementedException();
            return new PasswordChangeRoutine(null);
        }

        public static async Task<VaultRoutines> CreateRoutineAsync(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken = default)
        {
            var vaultValidator = new VaultValidator(serializer);
            var validationResult = await vaultValidator.ValidateAsync(vaultFolder, cancellationToken);
            if (!validationResult.Successful)
                throw validationResult.Exception ?? new InvalidDataException("Vault folder was not valid.");

            return new VaultRoutines(vaultFolder, serializer);
        }
    }
}
