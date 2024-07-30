using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    public sealed partial class CredentialsConfirmationViewModel : ObservableObject
    {
        [ObservableProperty] private bool _IsRemoving;
        [ObservableProperty] private bool _CanComplement;
        [ObservableProperty] private bool _IsComplementing;
        [ObservableProperty] private AuthenticationViewModel? _SelectedViewModel;

        public CredentialsConfirmationViewModel(AuthenticationViewModel selectedViewModel)
        {
            _SelectedViewModel = selectedViewModel;
        }

        [RelayCommand]
        private async Task ChosenAsync() // TODO: Name tbd
        {
            await Task.CompletedTask;
        }
    }
}
