using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    [Inject<IVaultManagerService>, Inject<IVaultService>]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly AuthenticationStage _authenticationStage;
        private readonly TaskCompletionSource<IKeyUsage> _credentialsTcs;

        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private bool _IsComplementationAvailable;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private AuthenticationViewModel? _ConfiguredViewModel;

        public required IDisposable UnlockContract { private get; init; }

        public KeySequence? OldPasskey { private get; init; }

        public IReadOnlyList<string>? OldAuthenticationMethodIds { private get; init; }

        public CredentialsConfirmationViewModel(IFolder vaultFolder, RegisterViewModel registerViewModel, AuthenticationStage authenticationStage)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            _RegisterViewModel = registerViewModel;
            _authenticationStage = authenticationStage;
            _credentialsTcs = new();

            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
        }

        [RelayCommand]
        public async Task ConfirmAsync(CancellationToken cancellationToken)
        {
            if (IsRemoving)
                await RemoveAsync(cancellationToken);
            else
                await ModifyAsync(cancellationToken);
        }

        private async Task ModifyAsync(CancellationToken cancellationToken)
        {
            RegisterViewModel.ConfirmCredentialsCommand.Execute(null);
            var key = await _credentialsTcs.Task;
            var configuredOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var authenticationMethod = GetAuthenticationMethod();

            await ChangeCredentialsAsync(key, configuredOptions, authenticationMethod, cancellationToken);
            return;

            AuthenticationMethod GetAuthenticationMethod()
            {
                ArgumentNullException.ThrowIfNull(RegisterViewModel.CurrentViewModel);
                return IsComplementing
                    ? _authenticationStage switch
                    {
                        AuthenticationStage.ProceedingStageOnly => new AuthenticationMethod([configuredOptions.UnlockProcedure.Methods[0]], RegisterViewModel.CurrentViewModel.Id),
                        AuthenticationStage.FirstStageOnly => throw new InvalidOperationException(),
                        _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
                    }
                    : _authenticationStage switch
                    {
                        AuthenticationStage.ProceedingStageOnly => new AuthenticationMethod([configuredOptions.UnlockProcedure.Methods[0], RegisterViewModel.CurrentViewModel.Id], null),
                        AuthenticationStage.FirstStageOnly => configuredOptions.UnlockProcedure with { Methods = configuredOptions.UnlockProcedure.Methods.SetAndGet(0, RegisterViewModel.CurrentViewModel.Id) },
                        _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
                    };
            }
        }

        private async Task RemoveAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(OldPasskey);
            if (_authenticationStage != AuthenticationStage.ProceedingStageOnly)
                return;

            var firstStageKey = OldPasskey.Keys.First();
            var configuredOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var authenticationMethod = new AuthenticationMethod([configuredOptions.UnlockProcedure.Methods[0]], null);

            await ChangeCredentialsAsync(firstStageKey, configuredOptions, authenticationMethod, cancellationToken);
        }

        private async Task ChangeCredentialsAsync(IKeyUsage key, VaultOptions configuredOptions, AuthenticationMethod unlockProcedure, CancellationToken cancellationToken)
        {
            // Modify the current unlock procedure
            var updatedOptions = configuredOptions with
            {
                UnlockProcedure = unlockProcedure
            };

            if (OldPasskey is not null)
            {
                if (RequiresComplementationRoutine(configuredOptions.UnlockProcedure, unlockProcedure))
                    await VaultManagerService.ModifyComplementationAsync(_vaultFolder, UnlockContract, CreateComplementationCredentials(key, configuredOptions.UnlockProcedure, unlockProcedure), updatedOptions, cancellationToken);
                else
                    await VaultManagerService.ModifyAuthenticationAsync(_vaultFolder, UnlockContract, OldPasskey, key, updatedOptions, cancellationToken);
            }
            else
                await VaultManagerService.ModifyAuthenticationAsync(_vaultFolder, UnlockContract, key, updatedOptions, cancellationToken);

            if (!string.IsNullOrEmpty(configuredOptions.VaultId))
                PersistedCredentialsModel.Instance.Remove(configuredOptions.VaultId);

            // Revoke (invalidate) old configured credentials if those are different from newly configured ones.
            // If both are the same, the authentication method should override the old ones; otherwise we would be deleting
            // the reconfigured ones, essentially breaking the vault!
            if (RegisterViewModel.CurrentViewModel is not null
                && ConfiguredViewModel is not null
                && !RegisterViewModel.CurrentViewModel.Id.Equals(ConfiguredViewModel.Id))
                await ConfiguredViewModel.RevokeAsync(configuredOptions.VaultId, cancellationToken);
        }

        private ComplementationCredentials CreateComplementationCredentials(IKeyUsage key, AuthenticationMethod configuredProcedure, AuthenticationMethod updatedProcedure)
        {
            ArgumentNullException.ThrowIfNull(OldPasskey);

            var oldComplementation = configuredProcedure.Complementation;
            var newComplementation = updatedProcedure.Complementation;
            var primaryChanged = !configuredProcedure.Methods.SequenceEqual(updatedProcedure.Methods);
            var primaryMethod = GetPrimaryMethod(configuredProcedure);
            var currentPrimaryCredential = GetOldCredentialByMethod(configuredProcedure, primaryMethod);
            var currentComplementCredential = string.IsNullOrWhiteSpace(oldComplementation)
                ? null
                : GetOldCredentialByMethod(configuredProcedure, oldComplementation);

            if (string.IsNullOrWhiteSpace(oldComplementation) && !string.IsNullOrWhiteSpace(newComplementation))
            {
                return new()
                {
                    CurrentKeystoreCredential = OldPasskey,
                    CurrentPrimaryCredential = currentPrimaryCredential,
                    NewPrimaryCredential = primaryChanged ? GetCredentialAt(key, 0) ?? key : null,
                    NewComplementCredential = GetCredentialAt(key, 1) ?? key
                };
            }

            if (!string.IsNullOrWhiteSpace(oldComplementation) && string.IsNullOrWhiteSpace(newComplementation))
            {
                return new()
                {
                    CurrentPrimaryCredential = currentPrimaryCredential,
                    NewPrimaryCredential = updatedProcedure.Methods.Length > 1 ? key : null
                };
            }

            if (!string.IsNullOrWhiteSpace(oldComplementation) && !string.IsNullOrWhiteSpace(newComplementation))
            {
                var updatePrimaryCredential = primaryChanged || _authenticationStage == AuthenticationStage.FirstStageOnly;
                var updateComplementCredential = !string.Equals(oldComplementation, newComplementation, StringComparison.Ordinal) ||
                                                 _authenticationStage == AuthenticationStage.ProceedingStageOnly;

                return new()
                {
                    CurrentPrimaryCredential = currentPrimaryCredential,
                    CurrentComplementCredential = currentComplementCredential,
                    NewPrimaryCredential = updatePrimaryCredential ? GetCredentialAt(key, 0) ?? key : null,
                    NewComplementCredential = updateComplementCredential ? GetCredentialAt(key, 1) ?? key : null
                };
            }

            throw new InvalidOperationException("The requested authentication change does not involve complementation.");
        }

        private static bool RequiresComplementationRoutine(AuthenticationMethod configuredProcedure, AuthenticationMethod updatedProcedure)
        {
            var wasComplemented = !string.IsNullOrWhiteSpace(configuredProcedure.Complementation);
            var willBeComplemented = !string.IsNullOrWhiteSpace(updatedProcedure.Complementation);
            var complementationChanged = !string.Equals(configuredProcedure.Complementation, updatedProcedure.Complementation, StringComparison.Ordinal);

            return complementationChanged || wasComplemented || willBeComplemented;
        }

        private static IKeyUsage? GetCredentialAt(IKeyUsage key, int index)
        {
            return key is KeySequence sequence
                ? sequence.Keys.ElementAtOrDefault(index)
                : index == 0 ? key : null;
        }

        private IKeyUsage? GetOldCredentialByMethod(AuthenticationMethod configuredProcedure, string authenticationMethodId)
        {
            var methodIds = GetOldAuthenticationMethodIds(configuredProcedure);
            if (methodIds is null)
                return null;

            for (var i = 0; i < methodIds.Count; i++)
            {
                if (string.Equals(methodIds[i], authenticationMethodId, StringComparison.Ordinal))
                    return GetCredentialAt(OldPasskey!, i);
            }

            return null;
        }

        private IReadOnlyList<string>? GetOldAuthenticationMethodIds(AuthenticationMethod configuredProcedure)
        {
            if (OldAuthenticationMethodIds is { Count: > 0 })
                return OldAuthenticationMethodIds;

            return string.IsNullOrWhiteSpace(configuredProcedure.Complementation)
                ? configuredProcedure.Methods
                : null;
        }

        private static string GetPrimaryMethod(AuthenticationMethod authenticationMethod)
        {
            return authenticationMethod.Methods.FirstOrDefault() ?? throw new InvalidOperationException("Primary authentication is missing.");
        }

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentialsTcs.TrySetResult(e.Authentication);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RegisterViewModel.CredentialsProvided -= RegisterViewModel_CredentialsProvided;
        }
    }
}
