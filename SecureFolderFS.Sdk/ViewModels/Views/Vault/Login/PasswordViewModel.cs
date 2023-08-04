using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Shared.Utilities;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Login
{
    public sealed partial class PasswordViewModel : BaseLoginViewModel
    {
        [ObservableProperty] private bool _ShowInvalidPassword;

        public PasswordViewModel(AuthenticationModel authenticationModel)
            : base(authenticationModel)
        {
        }

        /// <inheritdoc/>
        protected override void SetError(IResult? result)
        {
            ShowInvalidPassword = !result?.Successful ?? false;
        }

        [RelayCommand]
        private void ProvideCredentials(IPassword? password)
        {
            if (password is null || password.Length == 0)
                return;

            InvokeStateChanged(this, new AuthenticationChangedEventArgs(password));
        }
    }
}
