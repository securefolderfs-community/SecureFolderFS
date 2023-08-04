using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Login
{
    public sealed partial class ErrorViewModel : ObservableObject
    {
        [ObservableProperty] private string? _ErrorMessage;

        public ErrorViewModel(string errorMessage)
        {
            _ErrorMessage = errorMessage;
        }
    }
}
