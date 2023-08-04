using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Login
{
    public sealed partial class AuthenticationViewModel : BaseLoginViewModel
    {
        [ObservableProperty] private string? _ErrorMessage;
        [ObservableProperty] private string? _AuthenticationText;

        public AuthenticationViewModel(AuthenticationModel authenticationModel)
            : base(authenticationModel)
        {
            _AuthenticationText = string.Format("AuthenticateUsing".ToLocalized(), authenticationModel.AuthenticationName);
        }

        /// <inheritdoc/>
        protected override void SetError(IResult? result)
        {
            ErrorMessage = result?.GetMessage();
        }

        [RelayCommand]
        private async Task AuthenticateUserAsync(CancellationToken cancellationToken)
        {
            _ = authenticationModel.Authenticator ?? throw new InvalidOperationException("Could not authenticate");

            try
            {
                var identity = await authenticationModel.Authenticator.AuthenticateAsync(cancellationToken);
                if (identity is not IDisposable disposable)
                    return;

                InvokeStateChanged(this, new AuthenticationChangedEventArgs(disposable));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
