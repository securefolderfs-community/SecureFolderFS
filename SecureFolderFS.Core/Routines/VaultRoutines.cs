using SecureFolderFS.Core.DataModels;
using SecureFolderFS.Core.Routines.PasswordChangeRoutines;
using SecureFolderFS.Core.Validators;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Utils;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Core.Routines
{
    public static class VaultRoutines
    {
        public static async Task<IResult<IPasswordChangeRoutine>> NewPasswordChangeRoutineAsync(IFolder vaultFolder, IAsyncSerializer<Stream> serializer, CancellationToken cancellationToken)
        {
            var vaultValidator = new VaultValidator(serializer);
            var validationResult = await vaultValidator.ValidateAsync(vaultFolder, cancellationToken);
            if (!validationResult.Successful)
                return new CommonResult<IPasswordChangeRoutine>(validationResult.Exception);

            if (validationResult is not IResult<VaultConfigurationDataModel> configResult || configResult.Value is null)
                return new CommonResult<IPasswordChangeRoutine>(new InvalidCastException($"Cannot cast {nameof(validationResult)} to {nameof(IResult<VaultConfigurationDataModel>)}."));

            var routine = configResult.Value.Version switch
            {
                Constants.VaultVersion.LATEST_VERSION => new PasswordChangeRoutine(configResult.Value),
                _ => null
            };

            if (routine is null)
                return new CommonResult<IPasswordChangeRoutine>(new NotSupportedException($"Routine for vault version {configResult.Value.Version} is not supported."));

            return new CommonResult<IPasswordChangeRoutine>(routine);
        }
    }
}
