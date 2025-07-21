using OwlCore.Storage;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class ValidationHelpers
    {
        public static async Task<IResultWithMessage<Severity>> ValidateExistingVault(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);

            if (!validationResult.Successful)
            {
                if (validationResult.Exception is NotSupportedException)
                {
                    // Allow unsupported vaults to be migrated
                    return new MessageResult<Severity>(Severity.Warning, "SelectedMayNotBeSupported".ToLocalized());
                }

                return new MessageResult<Severity>(Severity.Critical, "SelectedInvalidVault".ToLocalized(), false);
            }

            return new MessageResult<Severity>(Severity.Success, "SelectedValidVault".ToLocalized());
        }

        public static async Task<IResultWithMessage<Severity>> ValidateNewVault(IFolder vaultFolder, CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);
            if (validationResult.Successful || validationResult.Exception is NotSupportedException)
            {
                // Check if a valid (or unsupported) vault exists at a specified path
                return new MessageResult<Severity>(Severity.Warning, "SelectedToBeOverwritten".ToLocalized());
            }

            return new MessageResult<Severity>(Severity.Success, "SelectedWillCreate".ToLocalized());
        }

        public static PasswordStrength ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return PasswordStrength.Blank;

            var score = (uint)PasswordStrength.VeryWeak; // Start at VeryWeak

            if (password.Length >= 8)
                score++;

            if (password.Length >= 10)
                score++;

            var hasDigit = password.Any(char.IsDigit);
            var hasUpper = password.Any(c => char.IsLetter(c) && char.IsUpper(c));
            var hasLower = password.Any(c => char.IsLetter(c) && char.IsLower(c));
            var hasSpecial = password.Any(c => !char.IsLetterOrDigit(c));

            if (hasDigit && hasUpper && hasLower)
                score++;

            if (hasSpecial)
                score++;

            // Clamp to max enum value
            score = Math.Min(score, (int)PasswordStrength.VeryStrong);
            return (PasswordStrength)score;
        }
    }
}
