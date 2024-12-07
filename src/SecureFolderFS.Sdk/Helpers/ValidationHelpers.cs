using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class ValidationHelpers
    {
        public static async Task<IResultWithMessage<SeverityType>> ValidateExistingVault(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);

            if (!validationResult.Successful)
            {
                if (validationResult.Exception is NotSupportedException)
                {
                    // Allow unsupported vaults to be migrated
                    return new MessageResult<SeverityType>(SeverityType.Warning, "SelectedMayNotBeSupported".ToLocalized());
                }

                return new MessageResult<SeverityType>(SeverityType.Critical, "SelectedInvalidVault".ToLocalized(), false);
            }

            return new MessageResult<SeverityType>(SeverityType.Success, "SelectedValidVault".ToLocalized());
        }

        public static async Task<IResultWithMessage<SeverityType>> ValidateNewVault(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);
            if (validationResult.Successful || validationResult.Exception is NotSupportedException)
            {
                // Check if a valid (or unsupported) vault exists at a specified path
                return new MessageResult<SeverityType>(SeverityType.Warning, "SelectedToBeOverwritten".ToLocalized());
            }

            return new MessageResult<SeverityType>(SeverityType.Success, "SelectedWillCreate".ToLocalized());
        }
    }
}
