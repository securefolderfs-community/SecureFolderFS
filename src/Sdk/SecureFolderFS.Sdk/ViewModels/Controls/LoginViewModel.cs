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
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SecureFolderFS.Sdk.ViewModels.Controls
{
    [Inject<IVaultService>, Inject<IVaultManagerService>, Inject<IVaultCredentialsService>, Inject<IOverlayService>]
    [Bindable(true)]
    public sealed partial class LoginViewModel : ObservableObject, IViewable, IAsyncInitialize, IDisposable
    {
        private readonly IFolder _vaultFolder;
        private readonly KeySequence _keySequence;
        private readonly LoginViewType _loginViewMode;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private Iterator<AuthenticationViewModel>? _loginSequence;

        [ObservableProperty] private string? _Title;
        [ObservableProperty] private bool _CanRecover;
        [ObservableProperty] private bool _IsLoginSequence;
        [ObservableProperty] private ICommand? _ProvideCredentialsCommand;
        [ObservableProperty] private ReportableViewModel? _CurrentViewModel;

        public event EventHandler<VaultUnlockedEventArgs>? VaultUnlocked;

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
                    // Get the authentication method enumerator for this vault
                    var loginItems = await VaultCredentialsService.GetLoginAsync(_vaultFolder, cancellationToken).ToArrayAsync(cancellationToken);
                    _loginSequence = new(loginItems);
                    IsLoginSequence = _loginSequence.Count > 1;

                    // Set up the first authentication method
                    var result = ProceedAuthentication();
                    if (!result.Successful)
                        CurrentViewModel = new ErrorViewModel(result);
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
                    // TODO: Report to user
                    _ = ex;
                }
            }
        }

        [RelayCommand]
        private void RestartLoginProcess()
        {
            // Dispose built key sequence
            _keySequence.Dispose();

            // Reset login sequence only if chain is longer than one authentication
            if (_loginSequence?.Count > 1)
            {
                _loginSequence?.Reset();
                var result = ProceedAuthentication();
                if (!result.Successful)
                    CurrentViewModel = new ErrorViewModel(result);
            }
        }

        private async Task<bool> TryUnlockAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var unlockContract = await VaultManagerService.UnlockAsync(_vaultFolder, _keySequence, cancellationToken);
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

        private IResult ProceedAuthentication()
        {
            try
            {
                if (_loginSequence is null || !_loginSequence.MoveNext())
                    return new MessageResult(false, "No authentication methods available.");

                // Get the appropriate method
                var viewModel = _loginSequence.Current;
                CurrentViewModel = viewModel;

                return Result.Success;
            }
            catch (Exception ex)
            {
                return Result.Failure(ex);
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
                // Add authentication
                _keySequence.Add(e.Authentication);

                var result = ProceedAuthentication();
                if (!result.Successful && CurrentViewModel is not ErrorViewModel)
                {
                    // Reached the end in which case we should try to unlock the vault
                    if (!await TryUnlockAsync())
                    {
                        // If login failed, restart the process
                        RestartLoginProcess();
                    }
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
