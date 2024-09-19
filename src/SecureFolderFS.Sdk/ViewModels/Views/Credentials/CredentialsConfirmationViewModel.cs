using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    [Inject<IVaultManagerService>]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject, IDisposable
    {
        private readonly AuthenticationType _authenticationStage;

        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _CanComplement;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;
        [ObservableProperty] private AuthenticationViewModel? _AuthenticationViewModel;

        public CredentialsConfirmationViewModel(AuthenticationViewModel authenticationViewModel, AuthenticationType authenticationStage)
        {
            ServiceProvider = DI.Default;
            _AuthenticationViewModel = authenticationViewModel;
            _authenticationStage = authenticationStage;
            _RegisterViewModel = new()
            {
                CurrentViewModel = AuthenticationViewModel
            };
        }

        [RelayCommand]
        private async Task ConfirmAsync()
        {
            await Task.CompletedTask;
        }

        [RelayCommand]
        private async Task RemoveAsync()
        {
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RegisterViewModel.Dispose();
        }
    }
}
