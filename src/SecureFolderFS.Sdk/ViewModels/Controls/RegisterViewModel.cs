using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Helpers;
using SecureFolderFS.Shared.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Bindable(true)]
    public sealed partial class RegisterViewModel : ObservableObject, IDisposable
    {
        private readonly AuthenticationStage _authenticationStage;
        private bool _credentialsAdded;

        [ObservableProperty] private bool _CanContinue;
        [ObservableProperty] private AuthenticationViewModel? _CurrentViewModel;

        /// <summary>
        /// Occurs when credentials have been provided by the user.
        /// </summary>
        public event EventHandler<CredentialsProvidedEventArgs>? CredentialsProvided;

        /// <summary>
        /// Gets the user credentials.
        /// </summary>
        public KeySequence Credentials { get; }

        public RegisterViewModel(AuthenticationStage authenticationStage, KeySequence? credentials = null)
        {
            _authenticationStage = authenticationStage;
            Credentials = credentials ?? new();
        }

        public async Task RevokeCredentialsAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!_credentialsAdded)
                    return;

                if (CurrentViewModel is null)
                    return;

                await CurrentViewModel.RevokeAsync(null, cancellationToken);
                Credentials.RemoveAt(_authenticationStage == AuthenticationStage.FirstStageOnly ? 0 : 1);
                CanContinue = false;
                _credentialsAdded = false;
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        }

        [RelayCommand]
        private void ConfirmCredentials()
        {
            // In case the authentication was not reported, try to extract it manually, if possible
            if (!_credentialsAdded && CurrentViewModel is IWrapper<IKey> keyWrapper)
            {
                Credentials.SetOrAdd(_authenticationStage == AuthenticationStage.FirstStageOnly ? 0 : 1, keyWrapper.Inner);
                _credentialsAdded = true;
            }

            if (Credentials.Count == 0)
                return;

            CredentialsProvided?.Invoke(this, new(Credentials));
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
                if (Credentials.Count > 0)
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
            _credentialsAdded = true;
            Credentials.SetOrAdd(_authenticationStage == AuthenticationStage.FirstStageOnly ? 0 : 1, e.Authentication);
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
            Credentials.Dispose();
            CredentialsProvided = null;
            if (CurrentViewModel is not null)
            {
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;
                CurrentViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;
            }
        }
    }
}
