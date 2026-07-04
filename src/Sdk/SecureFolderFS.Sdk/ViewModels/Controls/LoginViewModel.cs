using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;
using SecureFolderFS.Shared.SecureStore;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>, Inject<IVaultManagerService>, Inject<IVaultCredentialsService>, Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class LoginViewModel : ObservableObject, IViewable, IAsyncInitialize, INotifyStateChanged, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly KeySequence _keySequence;
        private readonly LoginViewType _loginViewMode;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private VaultOptions? _vaultOptions;
        private Iterator<AuthenticationViewModel>? _loginSequence;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _CanRecover;
        [ObservableProperty] private bool _IsLoginSequence;
        [ObservableProperty] private bool _AreCredentialsSaved;
        [ObservableProperty] private bool _ShouldSaveCredentials;
        [ObservableProperty] private ICommand? _ProvideCredentialsCommand;
        [ObservableProperty] private ReportableViewModel? _CurrentViewModel;

        /// <summary>
        /// Occurs when a vault has been successfully unlocked.
        /// </summary>
        public event EventHandler<VaultUnlockedEventArgs>? VaultUnlocked;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? StateChanged;

        public LoginViewModel(IFolder vaultFolder, LoginViewType loginViewMode, KeySequence? keySequence = null)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            _loginViewMode = loginViewMode;
            _keySequence = keySequence ?? new();
            _vaultWatcherModel = new VaultWatcherModel(_vaultFolder);
            _vaultWatcherModel.StateChanged += VaultWatcherModel_StateChanged;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // TODO: Refactor choosing the login view model:
            //
            // 1. Expand vault validation to account for missing passkey files like windows_hello.cfg
            //      1a. Display an error view if not found
            //      1b. Offer to unlock (from error view) using recovery key, if possible
            // 2. (Optional) Add a 'provide keystore' view if the keystore is missing
            //      2a. After providing keystore, the presumed authentication should continue as normal
            //      2b. Offer to unlock (from 'provide keystore' view) using recovery key, if possible
            //

            // Dispose previous state, if any
            _keySequence.Dispose();
            _loginSequence?.Dispose();

            var validationResult = await VaultService.VaultValidator.TryValidateAsync(_vaultFolder, cancellationToken);
            if (validationResult.Successful)
            {
                try
                {
                    if (!PersistedCredentialsModel.Instance.Credentials.IsEmpty()
                        && _loginViewMode is LoginViewType.Full or LoginViewType.Constrained)
                    {
                        _vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
                        if (!string.IsNullOrEmpty(_vaultOptions.VaultId) && PersistedCredentialsModel.Instance.Credentials.ContainsKey(_vaultOptions.VaultId))
                        {
                            AreCredentialsSaved = true;
                            CurrentViewModel = new PersistedAuthenticationViewModel(_vaultOptions.VaultId);
                            return;
                        }
                    }

                    // Get the authentication method enumerator for this vault
                    var loginItems = await VaultCredentialsService.GetLoginAsync(_vaultFolder, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                    _loginSequence = new(loginItems);
                    IsLoginSequence = _loginSequence.Count > 1;

                    // Set up the first authentication method. At this point the sequence should advance to the
                    // first method; a Completed step means the vault has no usable authentication methods
                    CurrentViewModel = ProceedAuthentication(out var result) switch
                    {
                        AuthenticationStep.Faulted => new ErrorViewModel(result),
                        AuthenticationStep.Completed => new ErrorViewModel(new MessageResult(false,
                            "No authentication methods available.")),
                        _ => CurrentViewModel
                    };
                }
                catch (NotSupportedException)
                {
                    // Catch any errors if a given authentication method is unsupported
                    CurrentViewModel = new UnsupportedViewModel("AuthenticationUnavailableOrExpired".ToLocalized());
                }
                catch (Exception ex)
                {
                    // Default to an error view
                    CurrentViewModel = new ErrorViewModel(Result.Failure(ex));
                }
            }
            else
            {
                // Try to migrate the vault if not supported
                if (validationResult.Exception is NotSupportedException ex && ex.Data["Version"] is int currentVersion)
                {
                    CanRecover = false;
                    CurrentViewModel = _loginViewMode switch
                    {
                        LoginViewType.Full => new MigrationViewModel(_vaultFolder, currentVersion) { Title = Title },
                        _ => new ErrorViewModel("You'll need to migrate this vault before it can be used.") // TODO: Localize
                    };
                }
                else
                    CurrentViewModel = new ErrorViewModel(validationResult);
            }

            // TODO: VaultWatcherModel.InitAsync is never called
        }

        [RelayCommand]
        private async Task RecoverAccessAsync(string? recoveryKey, CancellationToken cancellationToken)
        {
            if (recoveryKey is null)
            {
                var recoveryOverlay = new RecoveryOverlayViewModel(_vaultFolder);
                var result = await OverlayService.ShowAsync(recoveryOverlay);
                if (!result.Positive() || recoveryOverlay.UnlockContract is null)
                {
                    recoveryOverlay.Dispose();
                    return;
                }

                VaultUnlocked?.Invoke(this, new(recoveryOverlay.UnlockContract, _vaultFolder, true));
            }
            else
            {
                try
                {
                    var unlockContract = await VaultManagerService.RecoverAsync(_vaultFolder, recoveryKey, cancellationToken);
                    VaultUnlocked?.Invoke(this, new(unlockContract, _vaultFolder, true));
                }
                catch (Exception ex)
                {
                    // Notify both the current authentication view and any listening host
                    var result = Result.Failure(ex);
                    CurrentViewModel?.Report(result);
                    StateChanged?.Invoke(this, new ErrorReportedEventArgs(result));
                }
            }
        }

        [RelayCommand]
        private async Task DiscardSavedCredentialsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
                if (!string.IsNullOrEmpty(vaultOptions.VaultId))
                {
                    PersistedCredentialsModel.Instance.Remove(vaultOptions.VaultId);
                    AreCredentialsSaved = false;

                    _loginSequence?.Dispose();
                    var loginItems = await VaultCredentialsService.GetLoginAsync(_vaultFolder, cancellationToken).ToArrayAsyncImpl(cancellationToken);
                    _loginSequence = new(loginItems);
                    IsLoginSequence = _loginSequence.Count > 1;
                    RestartLoginProcess();
                }
            }
            catch (Exception ex)
            {
                CurrentViewModel = new ErrorViewModel(Result.Failure(ex));
            }
        }

        [RelayCommand]
        private void RestartLoginProcess()
        {
            // Dispose built key sequence
            _keySequence.Dispose();
            _loginSequence?.Reset();
            CurrentViewModel = ProceedAuthentication(out var result) switch
            {
                AuthenticationStep.Faulted => new ErrorViewModel(result),
                AuthenticationStep.Completed => new ErrorViewModel(new MessageResult(false, "No authentication methods available.")),
                _ => CurrentViewModel
            };
        }

        private async Task<bool> TryUnlockAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var unlockContract = await VaultManagerService.UnlockAsync(_vaultFolder, _keySequence, cancellationToken);
                _vaultOptions ??= await VaultService.GetVaultOptionsAsync(_vaultFolder, cancellationToken);
                if (string.IsNullOrWhiteSpace(_vaultOptions.VaultId))
                {
                    VaultUnlocked?.Invoke(this, new(unlockContract, _vaultFolder, false));
                    return true;
                }

                if (_loginViewMode is LoginViewType.Full or LoginViewType.Constrained && ShouldSaveCredentials && !AreCredentialsSaved)
                {
                    byte[]? keySequenceCopy = null;
                    _keySequence.UseKey(k => keySequenceCopy = k.ToArray());
                    if (keySequenceCopy is not null)
                        PersistedCredentialsModel.Instance.SetOrAdd(_vaultOptions.VaultId, SecureKey.TakeOwnership(keySequenceCopy));
                }

                VaultUnlocked?.Invoke(this, new(unlockContract, _vaultFolder, false));
                return true;
            }
            catch (Exception ex)
            {
                // Report that an error occurred when trying to log in
                CurrentViewModel?.Report(Result.Failure(ex));
                return false;
            }
        }

        /// <summary>
        /// Advances the authentication sequence to the next method.
        /// </summary>
        /// <param name="result">
        /// When the step is <see cref="AuthenticationStep.Faulted"/>, contains the failure; otherwise <see cref="Result.Success"/>.
        /// </param>
        /// <returns>The outcome of advancing the sequence.</returns>
        private AuthenticationStep ProceedAuthentication(out IResult result)
        {
            result = Result.Success;
            try
            {
                // MoveNext returning false is the legitimate end-of-sequence signal, not an error
                if (_loginSequence is null || !_loginSequence.MoveNext())
                    return AuthenticationStep.Completed;

                // Get the appropriate method
                var viewModel = _loginSequence.Current;
                CurrentViewModel = viewModel;

                return AuthenticationStep.Advanced;
            }
            catch (Exception ex)
            {
                result = Result.Failure(ex);
                return AuthenticationStep.Faulted;
            }
        }

        private async void VaultWatcherModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is VaultChangedEventArgs { ContentsChanged: true })
            {
                var result = await VaultService.VaultValidator.TryValidateAsync(_vaultFolder);
                if (result.Successful)
                    return;

                CurrentViewModel = new ErrorViewModel(result);
            }
        }

        private async void CurrentViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is CredentialsProvisionChangedEventArgs provisionArgs)
            {
                // TODO: Implement provisioning
                _ = provisionArgs.ClearProvision;
                _ = provisionArgs.SignedProvision;
            }
            else if (e is MigrationCompletedEventArgs)
            {
                await InitAsync();
            }
        }

        private async void CurrentViewModel_CredentialsProvided(object? sender, CredentialsProvidedEventArgs e)
        {
            try
            {
                // If authentication is empty, restart the process
                if (e.Authentication.Length == 0)
                {
                    e.Authentication.Dispose();
                    await DiscardSavedCredentialsAsync(CancellationToken.None);
                    return;
                }

                // Add authentication
                _keySequence.Add(e.Authentication);

                var step = ProceedAuthentication(out var result);
                switch (step)
                {
                    case AuthenticationStep.Faulted:
                        // A genuine error while advancing - report it
                        CurrentViewModel?.Report(result);
                        break;

                    case AuthenticationStep.Completed:
                        // Reached the end of the sequence - try to unlock the vault
                        if (!await TryUnlockAsync())
                            RestartLoginProcess();
                        break;

                    // AuthenticationStep.Advanced: the next method's view is now shown, nothing to do
                }
            }
            finally
            {
                e.TaskCompletion?.TrySetResult();
            }
        }

        partial void OnCurrentViewModelChanged(ReportableViewModel? oldValue, ReportableViewModel? newValue)
        {
            // Detach old
            if (oldValue is not null)
                oldValue.StateChanged -= CurrentViewModel_StateChanged;

            if (oldValue is AuthenticationViewModel oldViewModel)
                oldViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;

            // Attach new
            if (newValue is not null)
                newValue.StateChanged += CurrentViewModel_StateChanged;

            if (newValue is AuthenticationViewModel newViewModel)
            {
                newViewModel.CredentialsProvided += CurrentViewModel_CredentialsProvided;
                ProvideCredentialsCommand = newViewModel.ProvideCredentialsCommand;
            }
            else
                ProvideCredentialsCommand = null;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            VaultUnlocked = null;
            StateChanged = null;
            _vaultWatcherModel.StateChanged -= VaultWatcherModel_StateChanged;

            (CurrentViewModel as IDisposable)?.Dispose();
            if (CurrentViewModel is not null)
                CurrentViewModel.StateChanged -= CurrentViewModel_StateChanged;

            if (CurrentViewModel is AuthenticationViewModel authenticationViewModel)
                authenticationViewModel.CredentialsProvided -= CurrentViewModel_CredentialsProvided;

            _keySequence.Dispose();
            _loginSequence?.Dispose();
        }
    }
}
