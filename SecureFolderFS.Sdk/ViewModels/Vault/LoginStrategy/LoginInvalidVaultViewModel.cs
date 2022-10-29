using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy
{
    public sealed partial class LoginInvalidVaultViewModel : BaseLoginStrategyViewModel
    {
        [ObservableProperty]
        private string _ErrorMessage;

        public LoginInvalidVaultViewModel(string errorMessage)
        {
            _ErrorMessage = errorMessage;
        }
    }
}
