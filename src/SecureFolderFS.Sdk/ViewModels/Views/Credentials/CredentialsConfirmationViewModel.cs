using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

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
        public async Task ConfirmAsync(CancellationToken cancellationToken)
        {
            RegisterViewModel.ConfirmCredentialsCommand.Execute(null);
            var key = await _credentialsTcs.Task;
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
            var newOptions = new VaultOptions()
            {
                AuthenticationMethod = GetAuthenticationMethod(),
                ContentCipherId = vaultOptions.ContentCipherId,
                FileNameCipherId = vaultOptions.FileNameCipherId,
                VaultId = vaultOptions.VaultId,
                Version = vaultOptions.Version
            };
            
            await VaultManagerService.ChangeAuthenticationAsync(_vaultFolder, UnlockContract, key, newOptions, cancellationToken);
            return;

            string[] GetAuthenticationMethod()
            {
                ArgumentNullException.ThrowIfNull(RegisterViewModel.CurrentViewModel);
                return _authenticationStage switch
                {
                    AuthenticationType.ProceedingStageOnly => [vaultOptions.AuthenticationMethod[0], RegisterViewModel.CurrentViewModel.Id],
                    AuthenticationType.FirstStageOnly => vaultOptions.AuthenticationMethod.Length > 1
                        ? [RegisterViewModel.CurrentViewModel.Id, vaultOptions.AuthenticationMethod[1]]
                        : [RegisterViewModel.CurrentViewModel.Id],

                    _ => throw new ArgumentOutOfRangeException(nameof(_authenticationStage))
                };
            }
        }

        [RelayCommand]
        public async Task RemoveAsync()
        {
            await Task.CompletedTask;
        }

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentialsTcs.TrySetResult(e.Authentication);
        }
    }
}
