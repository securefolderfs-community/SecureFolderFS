using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    [Inject<IVaultManagerService>, Inject<IVaultService>]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject
    {
        private readonly IFolder _vaultFolder;
        private readonly AuthenticationType _authenticationStage;
        private readonly TaskCompletionSource<IKey> _credentialsTcs;

        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _CanComplement;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        
        public required IDisposable UnlockContract { private get; init; }

        public CredentialsConfirmationViewModel(IFolder vaultFolder, RegisterViewModel registerViewModel, AuthenticationType authenticationStage)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            _RegisterViewModel = registerViewModel;
            _authenticationStage = authenticationStage;
            _credentialsTcs = new();

            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
        }

        [RelayCommand]
        private async Task ConfirmAsync(CancellationToken cancellationToken)
        {
            var key = await _credentialsTcs.Task;
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var newOptions = new VaultOptions()
            {
                AuthenticationMethod = null, // TODO
                ContentCipherId = vaultOptions.ContentCipherId,
                FileNameCipherId = vaultOptions.FileNameCipherId,
                VaultId = vaultOptions.VaultId,
                Version = vaultOptions.Version
            };
            
            await VaultManagerService.ChangeAuthenticationAsync(_vaultFolder, UnlockContract, key, newOptions, cancellationToken);
        }

        [RelayCommand]
        private async Task RemoveAsync()
        {
            await Task.CompletedTask;
        }

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentialsTcs.TrySetResult(e.Authentication);
        }
    }
}
