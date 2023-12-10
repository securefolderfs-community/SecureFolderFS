using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.EventArguments;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Wizard.NewVault.Signup
{
    public sealed partial class AuthenticationWizardViewModel : BaseAuthWizardViewModel
    {
        private readonly string _vaultId;
        private IDisposable? _authentication;

        /// <inheritdoc/>
        public override event EventHandler<EventArgs>? StateChanged;

        public AuthenticationWizardViewModel(string vaultId, AuthenticationModel authenticationModel)
            : base(authenticationModel)
        {
            _vaultId = vaultId;
        }

        [RelayCommand]
        private async Task SetupAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (AuthenticationModel.Authenticator is null)
                return;

            _authentication?.Dispose();
            _authentication = null;
            _authentication = await AuthenticationModel.Authenticator.CreateAsync(_vaultId, null, cancellationToken);

            StateChanged?.Invoke(this, new AuthenticationChangedEventArgs(_authentication));
        }

        /// <inheritdoc/>
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
