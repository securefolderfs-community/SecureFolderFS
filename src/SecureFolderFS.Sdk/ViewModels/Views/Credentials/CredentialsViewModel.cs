using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Credentials
{
    [Bindable(true)]
    public sealed partial class CredentialsViewModel : ObservableObject
    {
        [ObservableProperty] private AuthenticationViewModel? _SelectedViewModel;

        [RelayCommand]
        private async Task ChosenAsync() // TODO: Name tbd
        {
            await Task.CompletedTask;
        }
    }
}
