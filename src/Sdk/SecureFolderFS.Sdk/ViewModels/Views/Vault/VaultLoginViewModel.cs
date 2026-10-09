using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OwlCore.Storage;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<ISettingsService>, Inject<IVaultManagerService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class VaultLoginViewModel : BaseDesignationViewModel, IVaultViewContext, INavigatable, IAsyncInitialize, IProgress<IResult>, IDisposable
    {
        private CancellationTokenSource? _connectionCts;

        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private bool _IsConnected;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private LoginViewModel? _LoginViewModel;
        [ObservableProperty] private InfoBarViewModel _StatusInfoBar = new();

        public INavigationService VaultNavigation { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel { get; }

        /// <inheritdoc/>
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public VaultLoginViewModel(VaultViewModel vaultViewModel, INavigationService vaultNavigation)
        {
            ServiceProvider = DI.Default;
            Title = vaultViewModel.Title;
            VaultNavigation = vaultNavigation;
            VaultViewModel = vaultViewModel;

            if (VaultViewModel.VaultModel.VaultFolder is { } vaultFolder)
                LoginViewModel = new(vaultFolder, LoginViewType.Full) { Title = vaultViewModel.Title };
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (LoginViewModel is null)
                return;

            try
            {
                IsConnected = VaultViewModel.VaultModel.IsRemote;
                LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
                IsProgressing = true;

                await Task.Delay(100, cancellationToken); // Wait for the UI to update
                await LoginViewModel.InitAsync(cancellationToken);
            }
            finally
            {
                IsProgressing = false;
            }

            #region Test for quick unlock on mobile
#if DEBUG
            if (VaultViewModel.VaultModel.VaultFolder is not { } vaultFolder)
                return;

            var recoveryKey = VaultViewModel.Title switch
            {
                "Vault V3" => "nn9oKIELbkAl3XevD/dhVhnBQcfkDA5wfLnY+aAUoK8=@@@w3jNIbmsDwThbNkuGqpVdCvCiU7RQHQtkEqGfBPqDRc=",
                "Plaintext Vault" => "lZbz5sWmeYDyyebm3LgPmvApNPsiyphj6zW4YZ2NuG8=@@@mEp3pOlUSXr0Yr47B+Se4M3ZXN8wPU/BlgFkLSpULiQ=",
                "TestFolder" => "AT690eDQi71F2pvBMOTW9tYaavdZxI5hF+in8GgVW+U=@@@zIMim+84MPoubryk2Ne9A8S5cXE18MOzLyET7yI6p/E=",
                "My Vault" => "R5blf8KbjkbPLjPvzFkZW3GPYDpNvzh1/QNhgmjQXrw=@@@BC4WxRPrXnaDSeu6jDioW4DYtKQzNlg0r7wyivxnZno=",
                _ => null
            };

            if (recoveryKey is null)
                return;

            var unlockContract = await VaultManagerService.RecoverAsync(vaultFolder, recoveryKey, cancellationToken);
            await Task.Delay(200);
            await UnlockAsync(unlockContract);
#endif
           #endregion
        }

        [RelayCommand]
        private async Task ConnectToVaultAsync(CancellationToken cancellationToken)
        {
            // Cancel any previous connection attempt and create a new CTS
            CancelConnection();
            _connectionCts = new CancellationTokenSource();

            // Link the command's token with our controllable token
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _connectionCts.Token);

            try
            {
                StatusInfoBar.IsOpen = false;
                LoginViewModel?.Dispose();
                var result = await VaultViewModel.VaultModel.TryConnectAsync(linkedCts.Token);
                if (!result.TryGetValue(out var vaultFolder))
                {
                    Report(MessageResult.WithMessage(result, "ConnectionFailed".ToLocalized()));
                    return;
                }

                IsProgressing = true;
                await Task.Delay(100, linkedCts.Token); // Wait for the UI to update

                LoginViewModel = new(vaultFolder, LoginViewType.Full) { Title = VaultViewModel.Title };
                await InitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException)
            {
                LoginViewModel?.Dispose();
                LoginViewModel = null;
            }
            catch (Exception ex)
            {
                Report(new MessageResult(ex, ex.Message));
            }
            finally
            {
                IsProgressing = false;
            }
        }

        [RelayCommand]
        private async Task DisconnectFromVaultAsync(CancellationToken cancellationToken)
        {
            await VaultViewModel.VaultModel.DisposeAsync();
            LoginViewModel?.Dispose();
            LoginViewModel = null;
            IsConnected = false;
        }

        [RelayCommand]
        private void CancelConnection()
        {
            if (_connectionCts is null)
                return;

            _connectionCts.TryCancel();
            _connectionCts.Dispose();
            _connectionCts = null;
        }

        [RelayCommand]
        private async Task BeginRecoveryAsync(CancellationToken cancellationToken)
        {
            if (VaultViewModel.VaultModel.VaultFolder is not { } vaultFolder)
                return;

            var recoveryOverlay = new RecoveryOverlayViewModel(vaultFolder);
            var result = await OverlayService.ShowAsync(recoveryOverlay);
            if (!result.Positive() || recoveryOverlay.UnlockContract is null)
            {
                recoveryOverlay.Dispose();
                return;
            }

            await UnlockAsync(recoveryOverlay.UnlockContract);
        }

        private async Task UnlockAsync(IDisposable unlockContract)
        {
            StatusInfoBar.IsOpen = false;

            UnlockedVaultViewModel unlockedVaultViewModel;
            try
            {
                unlockedVaultViewModel = await VaultViewModel.UnlockAsync(unlockContract, IsReadOnly);
            }
            catch (Exception ex)
            {
                // Mounting failed - the credentials were correct, so keep this view alive for a retry
                unlockContract.Dispose();
                Report(new MessageResult(ex, "UnlockFailed".ToLocalized()));

                // Reset the login state so the user can attempt to unlock again
                if (LoginViewModel is not null)
                    await LoginViewModel.InitAsync();

                return;
            }

            try
            {
                // Navigate away
                NavigationRequested?.Invoke(this, new UnlockNavigationRequestedEventArgs(unlockedVaultViewModel, this));

                // A restored vault is unlocked right away but has no credentials configured,
                // so the user is asked to set them up before anything else
                if (await RequiresCredentialsSetupAsync())
                    await SetUpCredentialsAsync(unlockedVaultViewModel.VaultFolder, unlockContract);

                // Show vault tutorial
                if (SettingsService.AppSettings.ShouldShowVaultTutorial)
                {
                    var explanationOverlay = new ExplanationOverlayViewModel();
                    await explanationOverlay.InitAsync();
                    await OverlayService.ShowAsync(explanationOverlay);

                    SettingsService.AppSettings.ShouldShowVaultTutorial = false;
                    await SettingsService.AppSettings.TrySaveAsync();
                }
            }
            finally
            {
                // Clean up the current instance
                Dispose();
            }
        }

        /// <summary>
        /// Determines whether the vault still awaits credentials, which is the case for a vault
        /// that was restored and can, for the time being, only be unlocked with its recovery key.
        /// </summary>
        /// <returns>A <see cref="Task"/> that represents the asynchronous operation. Value is true if credentials need to be set up; otherwise false.</returns>
        private async Task<bool> RequiresCredentialsSetupAsync()
        {
            if (VaultViewModel.VaultModel.VaultFolder is not { } vaultFolder)
                return false;

            try
            {
                var vaultOptions = await VaultService.GetVaultOptionsAsync(vaultFolder);
                return Array.IndexOf(vaultOptions.UnlockProcedure.Methods, Constants.Vault.Authentication.AUTH_RECOVERY_KEY_REQUIREMENT) >= 0;
            }
            catch (Exception)
            {
                // The vault is already unlocked at this point, so a failure here must not stand in the way
                return false;
            }
        }

        /// <summary>
        /// Shows the overlay that registers new credentials for the just unlocked vault.
        /// </summary>
        /// <remarks>
        /// Mirrors changing the first authentication from <see cref="VaultPropertiesViewModel"/>, except that
        /// the unlock contract is already at hand, so the recovery key does not have to be provided a second time.
        /// </remarks>
        private async Task SetUpCredentialsAsync(IFolder vaultFolder, IDisposable unlockContract)
        {
            if (IsReadOnly || OverlayService.CurrentView is not null)
                return;

            using var credentialsOverlay = new CredentialsOverlayViewModel(vaultFolder, VaultViewModel.Title, AuthenticationStage.FirstStageOnly, unlockContract);
            await credentialsOverlay.InitAsync();
            await OverlayService.ShowAsync(credentialsOverlay);
        }

        /// <inheritdoc/>
        public void Report(IResult result)
        {
            StatusInfoBar.Title = "ErrorOccurred".ToLocalized();
            StatusInfoBar.Message = result.GetMessage(result.Exception?.Message ?? "UnknownError".ToLocalized());
            StatusInfoBar.Severity = Severity.Critical;
            StatusInfoBar.IsCloseable = true;
            StatusInfoBar.IsOpen = true;
        }

        private void LoginViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is ErrorReportedEventArgs args)
                Report(MessageResult.WithMessage(args.Result, "RecoveryFailed".ToLocalized()));
        }

        partial void OnLoginViewModelChanged(LoginViewModel? oldValue, LoginViewModel? newValue)
        {
            oldValue?.StateChanged -= LoginViewModel_StateChanged;
            newValue?.StateChanged += LoginViewModel_StateChanged;
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            await UnlockAsync(e.UnlockContract);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _connectionCts?.TryCancel();
            _connectionCts?.Dispose();
            _connectionCts = null;
            IsConnected = false;
            LoginViewModel?.Dispose();
            NavigationRequested = null;
        }
    }
}
