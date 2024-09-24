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
        public static async Task<IResultWithMessage<ViewSeverityType>> ValidateExistingVault(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);

            if (!validationResult.Successful)
            {
                if (validationResult.Exception is NotSupportedException)
                {
                    // Allow unsupported vaults to be migrated
                    return new MessageResult<ViewSeverityType>(ViewSeverityType.Warning, "SelectedMayNotBeSupported".ToLocalized());
                }

                return new MessageResult<ViewSeverityType>(ViewSeverityType.Error, "SelectedInvalidVault".ToLocalized(), false);
            }

            return new MessageResult<ViewSeverityType>(ViewSeverityType.Success, "SelectedValidVault".ToLocalized());
        }

        public static async Task<IResultWithMessage<ViewSeverityType>> ValidateNewVault(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);
            if (validationResult.Successful || validationResult.Exception is NotSupportedException)
            {
                // Check if a valid (or unsupported) vault exists at a specified path
                return new MessageResult<ViewSeverityType>(ViewSeverityType.Warning, "SelectedToBeOverwritten".ToLocalized());
            }

            return new MessageResult<ViewSeverityType>(ViewSeverityType.Success, "SelectedWillCreate".ToLocalized());
        }
    }
}
