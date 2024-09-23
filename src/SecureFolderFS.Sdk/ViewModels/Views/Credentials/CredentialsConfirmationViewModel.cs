using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    [Inject<IVaultManagerService>]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject
    {
        private readonly AuthenticationType _authenticationStage;

        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _CanComplement;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private RegisterViewModel _RegisterViewModel;

        public CredentialsConfirmationViewModel(RegisterViewModel registerViewModel, AuthenticationType authenticationStage)
        {
            ServiceProvider = DI.Default;
            _RegisterViewModel = registerViewModel;
            _authenticationStage = authenticationStage;

            RegisterViewModel.CredentialsProvided += RegisterViewModel_CredentialsProvided;
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

        private void RegisterViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            throw new System.NotImplementedException();
        }
    }
}
