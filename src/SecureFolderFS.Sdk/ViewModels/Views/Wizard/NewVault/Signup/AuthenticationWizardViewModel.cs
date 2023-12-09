using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup
{
    public sealed partial class AuthenticationWizardViewModel : BaseAuthWizardViewModel
    {
        private IDisposable? _authentication;

        public AuthenticationWizardViewModel(AuthenticationModel authenticationModel)
            : base(authenticationModel)
        {
        }

        [RelayCommand]
        private async Task SetupAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (AuthenticationModel.Authenticator is null)
                return;

            _authentication?.Dispose();
            _authentication = null;
            _authentication = await AuthenticationModel.Authenticator.AuthenticateAsync("TODO", cancellationToken);
        }

        public override IDisposable? GetAuthentication()
        {
            return _authentication;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _authentication?.Dispose();
        }
    }
}
