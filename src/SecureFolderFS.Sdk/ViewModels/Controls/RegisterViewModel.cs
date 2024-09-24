using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>, Inject<IVaultManagerService>]
    [Bindable(true)]
    public sealed partial class RegisterViewModel : ObservableObject, IDisposable
    {
        private readonly KeyChain _credentials;

        [ObservableProperty] private bool _CanContinue;
        [ObservableProperty] private AuthenticationViewModel? _CurrentViewModel;

        /// <summary>
        /// Occurs when credentials have been provided by the user.
        /// </summary>
        public event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        public RegisterViewModel()
        {
            _credentials = new();
        }

        [RelayCommand]
        private void ConfirmCredentials()
        {
            // In case the authentication was not reported, try to extract it manually, if possible
            if (_credentials.Count == 0 && CurrentViewModel is IWrapper<IKey> keyWrapper)
                _credentials.Push(keyWrapper.Inner);

            if (_credentials.Count == 0)
                return;

            CredentialsProvided?.Invoke(this, new(_credentials));
        }

        async partial void OnCurrentViewModelChanged(AuthenticationViewModel? oldValue, AuthenticationViewModel? newValue)
        {
            // Make sure to dispose the old value in case there was authentication data provided
            oldValue?.Dispose();

            if (oldValue is not null)
            {
                oldValue.StateChanged -= CurrentViewModel_StateChanged;
                oldValue.CredentialsProvided -= CurrentViewModel_CredentialsProvided;

                // We also need to revoke existing credentials if the user added and aborted
                if (_credentials.Count > 0)
                    await SafetyHelpers.NoThrowAsync(async () => await oldValue.RevokeAsync(null));
            }

            if (newValue is not null)
            {
                newValue.StateChanged += CurrentViewModel_StateChanged;
                newValue.CredentialsProvided += CurrentViewModel_CredentialsProvided;
            }

            CanContinue = false;
        }

        private void CurrentViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            _credentials.Push(e.Authentication);
            CanContinue = true;
        }

        private void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is PasswordChangedEventArgs args)
                CanContinue = args.IsMatch;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _credentials.Dispose();
            CredentialsProvided = null;
            if (CurrentViewModel is not null)
            {
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;
                CurrentViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;
            }
        }
    }
}
