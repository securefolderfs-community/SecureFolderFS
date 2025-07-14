using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
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
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    [Inject<IVaultManagerService>, Inject<IVaultService>]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly AuthenticationStage _authenticationStage;
        private readonly TaskCompletionSource<IKey> _credentialsTcs;

        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private bool _IsComplementationAvailable;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private AuthenticationViewModel? _ConfiguredViewModel;

        public required IDisposable UnlockContract { private get; init; }

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
            if (_authenticationStage != AuthenticationStage.ProceedingStageOnly)
                return;

            var key = RegisterViewModel.Credentials.Keys.First();
            var configuredOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var authenticationMethod = new AuthenticationMethod([configuredOptions.UnlockProcedure.Methods[0]], null);

            await ChangeCredentialsAsync(key, configuredOptions, authenticationMethod, cancellationToken);
        }

        private async Task ChangeCredentialsAsync(IKey key, VaultOptions configuredOptions, AuthenticationMethod unlockProcedure, CancellationToken cancellationToken)
        {
            // Modify the current unlock procedure
            await VaultManagerService.ModifyAuthenticationAsync(_vaultFolder, UnlockContract, key, configuredOptions with
            {
                UnlockProcedure = unlockProcedure
            }, cancellationToken);

            // Revoke (invalidate) old configured credentials
            if (ConfiguredViewModel is not null)
                await ConfiguredViewModel.RevokeAsync(configuredOptions.VaultId, cancellationToken);
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
