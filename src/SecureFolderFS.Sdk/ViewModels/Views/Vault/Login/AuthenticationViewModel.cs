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
    public sealed partial class AuthenticationViewModel : ReportableViewModel
    {
        private readonly string _vaultId;
        private readonly AuthenticationModel _authenticationModel;

        [ObservableProperty] private string? _ErrorMessage;
        [ObservableProperty] private string? _AuthenticationText;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public AuthenticationViewModel(string vaultId, AuthenticationModel authenticationModel)
        {
            _vaultId = vaultId;
            _authenticationModel = authenticationModel;
            _AuthenticationText = string.Format("AuthenticateUsing".ToLocalized(), authenticationModel.Name);
        }

        /// <inheritdoc/>
        protected override void SetError(IResult? result)
        {
            ErrorMessage = result?.GetMessage();
        }

        [RelayCommand]
        private async Task AuthenticateUserAsync(CancellationToken cancellationToken)
        {
            _ = _authenticationModel.Authenticator ?? throw new InvalidOperationException("Could not authenticate.");

            try
            {
                var authentication = await _authenticationModel.Authenticator.SignAsync(_vaultId, null, cancellationToken);
                StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(authentication));
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
        }
    }
}
