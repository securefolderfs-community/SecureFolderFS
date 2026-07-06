using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Sdk.ViewModels.Views.Vault;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Models;

namespace SecureFolderFS.Sdk.ViewModels.Views.Root
{
    [Inject<IOverlayService>, Inject<ISettingsService>, Inject<IFileExplorerService>]
    [Bindable(true)]
    public sealed partial class VaultPreviewViewModel : ObservableObject, IAsyncInitialize, IRecipient<VaultUnlockedMessage>, IRecipient<VaultLockedMessage>, IProgress<IResult>, IDisposable
    {
        private readonly INavigationService _vaultNavigation;

        [ObservableProperty] private UnlockedVaultViewModel? _UnlockedVaultViewModel;
        [ObservableProperty] private LoginViewModel? _LoginViewModel;
        [ObservableProperty] private VaultViewModel _VaultViewModel;
        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private InfoBarViewModel _StatusInfoBar = new();

        public VaultPreviewViewModel(VaultViewModel vaultViewModel, INavigationService vaultNavigation)
        {
            ServiceProvider = DI.Default;
            _vaultNavigation = vaultNavigation;
            VaultViewModel = vaultViewModel;

            // Create login view model if vault is locked
            if (!vaultViewModel.IsUnlocked && vaultViewModel.VaultModel.VaultFolder is { } vaultFolder)
                LoginViewModel = new(vaultFolder, LoginViewType.Constrained);

            WeakReferenceMessenger.Default.Register<VaultUnlockedMessage>(this);
            WeakReferenceMessenger.Default.Register<VaultLockedMessage>(this);
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            if (LoginViewModel is null)
                return;

            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            await LoginViewModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public void Receive(VaultUnlockedMessage message)
        {
            if (UnlockedVaultViewModel is not null)
                return;

            if (!message.VaultModel.Equals(VaultViewModel.VaultModel))
                return;

            UnlockedVaultViewModel = VaultViewModel.GetUnlockedViewModel();
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            if (message.VaultModel != VaultViewModel.VaultModel)
                return;

            if (VaultViewModel.VaultModel.VaultFolder is null)
                return;

            LoginViewModel?.Dispose();
            LoginViewModel = new(VaultViewModel.VaultModel.VaultFolder, LoginViewType.Constrained) { Title = VaultViewModel.Title };
            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            _ = LoginViewModel.InitAsync();
        }

        [RelayCommand]
        private async Task RecoverAccessAsync(CancellationToken cancellationToken)
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

        [RelayCommand]
        private async Task RevealFolderAsync(CancellationToken cancellationToken)
        {
            if (UnlockedVaultViewModel is not null)
                await FileExplorerService.TryOpenInFileExplorerAsync(UnlockedVaultViewModel.StorageRoot.VirtualizedRoot, cancellationToken);
        }

        [RelayCommand]
        private async Task LockVaultAsync()
        {
            if (UnlockedVaultViewModel is null)
                return;

            try
            {
                // Lock vault
                await UnlockedVaultViewModel.DisposeAsync();
            }
            catch (Exception ex)
            {
                // Unmounting failed (e.g. files are still open) - the vault remains unlocked,
                // so don't navigate away or announce a lock that didn't happen
                Report(new MessageResult(ex, "LockFailed".ToLocalized()));
                return;
            }

            StatusInfoBar.IsOpen = false;

            // Prepare login page
            var loginPageViewModel = new VaultLoginViewModel(UnlockedVaultViewModel.VaultViewModel, _vaultNavigation);
            _ = loginPageViewModel.InitAsync();

            // Navigate away
            await _vaultNavigation.ForgetNavigateSpecificViewAsync(loginPageViewModel, x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(UnlockedVaultViewModel.VaultViewModel.VaultModel) ?? false);
            WeakReferenceMessenger.Default.Send(new VaultLockedMessage(UnlockedVaultViewModel.VaultViewModel.VaultModel));
        }

        private async Task UnlockAsync(IDisposable unlockContract)
        {
            StatusInfoBar.IsOpen = false;

            try
            {
                // Unlock the vault
                UnlockedVaultViewModel = await VaultViewModel.UnlockAsync(unlockContract, IsReadOnly);
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
                // Setup dashboard
                var dashboardNavigation = DI.Service<INavigationService>();
                var dashboardViewModel = new VaultDashboardViewModel(UnlockedVaultViewModel, _vaultNavigation, dashboardNavigation);

                // Navigate to dashboard
                await _vaultNavigation.ForgetNavigateSpecificViewAsync(
                    dashboardViewModel,
                    viewForgetter: x => (x as IVaultViewContext)?.VaultViewModel.VaultModel.Equals(VaultViewModel.VaultModel) ?? false,
                    addViewIfMissing: true);

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
                // Clean up login view model
                LoginViewModel?.Dispose();
                LoginViewModel = null;
            }
        }

        /// <inheritdoc/>
        public void Report(IResult result)
        {
            StatusInfoBar.Title = result.GetMessage("UnknownError".ToLocalized());
            StatusInfoBar.Severity = Severity.Critical;
            StatusInfoBar.IsCloseable = true;
            StatusInfoBar.IsOpen = true;
        }

        private void LoginViewModel_StateChanged(object? sender, EventArgs e)
        {
            if (e is ErrorReportedEventArgs args)
                Report(MessageResult.WithMessage(args.Result, "RecoveryFailed".ToLocalized()));
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            await UnlockAsync(e.UnlockContract);
        }

        partial void OnLoginViewModelChanged(LoginViewModel? oldValue, LoginViewModel? newValue)
        {
            oldValue?.StateChanged -= LoginViewModel_StateChanged;
            newValue?.StateChanged += LoginViewModel_StateChanged;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            if (LoginViewModel is not null)
            {
                LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
                LoginViewModel.StateChanged -= LoginViewModel_StateChanged;
                LoginViewModel.Dispose();
            }
        }
    }
}
