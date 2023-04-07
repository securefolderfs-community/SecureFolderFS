using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy
{
    public sealed partial class LoginErrorViewModel : BaseLoginStrategyViewModel
    {
        [ObservableProperty] private string _ErrorMessage;

        public LoginErrorViewModel(string errorMessage)
        {
            _ErrorMessage = errorMessage;
        }
    }
}
