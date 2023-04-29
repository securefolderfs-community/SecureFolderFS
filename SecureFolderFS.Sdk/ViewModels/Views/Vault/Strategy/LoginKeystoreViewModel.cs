using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy
{
    public sealed partial class LoginKeystoreViewModel : ObservableObject
    {
        [RelayCommand]
        private Task SelectKeystoreAsync()
        {
            return Task.CompletedTask;
        }
    }
}