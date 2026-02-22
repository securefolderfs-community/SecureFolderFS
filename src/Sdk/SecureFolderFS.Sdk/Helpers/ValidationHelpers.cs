using OwlCore.Storage;
using SecureFolderFS.Sdk.DataModels;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Storage.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.Helpers
{
    public static class ValidationHelpers
    {
        public static async Task<(Severity Severity, string? Message, string? SelectedLocation, bool CanContinue)> ValidateAddedVault(
            IFolder? selectedFolder,
            NewVaultMode mode,
            IEnumerable<VaultDataModel> existingVaults,
            CancellationToken cancellationToken)
        {
            Severity severity;
            string? message;
            string? selectedLocation;

            // No folder selected
            if (selectedFolder is null)
            {
                severity = Severity.Default;
                message = "SelectFolderToContinue".ToLocalized();
                selectedLocation = GetSelectedLocation();
                return (severity, message, selectedLocation, false);
            }

            // Check for duplicates
            var isDuplicate = existingVaults.Any(x => x.PersistableId == selectedFolder.GetPersistableId());
            if (isDuplicate)
            {
                severity = Severity.Warning;
                message = "VaultAlreadyExists".ToLocalized();
                selectedLocation = GetSelectedLocation();
                return (severity, message, selectedLocation, false);
            }

            // Validate vault
            var result = mode == NewVaultMode.AddExisting
                ? await ValidateExistingVault(selectedFolder, cancellationToken)
                : await ValidateNewVault(selectedFolder, cancellationToken);

            severity = result.Value;
            message = result.GetMessage("UnknownError".ToLocalized());
            selectedLocation = GetSelectedLocation();

            return (severity, message, selectedLocation, result.Successful);

            string GetSelectedLocation()
            {
                return selectedFolder is null ? "SelectedNone".ToLocalized() : selectedFolder.Name;
            }
        }

        public static async Task<IResultWithMessage<Severity>> ValidateExistingVault(
            IFolder vaultFolder,
            CancellationToken cancellationToken)
        {
            var vaultService = DI.Service<IVaultService>();
            var validationResult = await vaultService.VaultValidator.TryValidateAsync(vaultFolder, cancellationToken);
            if (validationResult.Successful)
                return new MessageResult<Severity>(Severity.Success, "SelectedValidVault".ToLocalized());

            return validationResult.Exception switch
            {
                NotSupportedException => new MessageResult<Severity>(Severity.Warning, "SelectedMayNotBeSupported".ToLocalized()),
                TimeoutException => new MessageResult<Severity>(Severity.Warning, "OperationTimedOut".ToLocalized(), false),
                _ => new MessageResult<Severity>(Severity.Critical, "SelectedInvalidVault".ToLocalized(), false)
            };
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
