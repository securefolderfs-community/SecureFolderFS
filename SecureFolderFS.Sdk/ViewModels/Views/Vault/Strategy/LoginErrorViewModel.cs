using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy
{
    public sealed partial class LoginErrorViewModel : ObservableObject
    {
        [ObservableProperty] private string? _Message;

        public LoginErrorViewModel(string message)
        {
            _Message = message;
        }
    }
}
