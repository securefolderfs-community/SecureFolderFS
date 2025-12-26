using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<ISettingsService>, Inject<IVaultManagerService>]
    [Bindable(true)]
    public sealed partial class VaultLoginViewModel : BaseDesignationViewModel, IVaultViewContext, INavigatable, IAsyncInitialize, IDisposable
    {
        private CancellationTokenSource? _connectionCts;

        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private bool _IsConnected;
        [ObservableProperty] private bool _IsProgressing;
        [ObservableProperty] private LoginViewModel? _LoginViewModel;

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

            IsConnected = VaultViewModel.VaultModel.IsRemote;
            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
            await LoginViewModel.InitAsync(cancellationToken);


            #region Test for quick unlock on mobile
#if DEBUG

            if (VaultViewModel.VaultModel.VaultFolder is not { } vaultFolder)
                return;

            var recoveryKey = VaultViewModel.Title switch
            {
                "Vault V3" => "nn9oKIELbkAl3XevD/dhVhnBQcfkDA5wfLnY+aAUoK8=@@@w3jNIbmsDwThbNkuGqpVdCvCiU7RQHQtkEqGfBPqDRc=",
                "Plaintext Vault" => "lZbz5sWmeYDyyebm3LgPmvApNPsiyphj6zW4YZ2NuG8=@@@mEp3pOlUSXr0Yr47B+Se4M3ZXN8wPU/BlgFkLSpULiQ=",
                "TestFolder" => "AT690eDQi71F2pvBMOTW9tYaavdZxI5hF+in8GgVW+U=@@@zIMim+84MPoubryk2Ne9A8S5cXE18MOzLyET7yI6p/E=",
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
                LoginViewModel?.Dispose();
                var result = await VaultViewModel.VaultModel.TryConnectAsync(linkedCts.Token);
                if (!result.TryGetValue(out var vaultFolder))
                {
                    // TODO: Report error
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

            _connectionCts.Cancel();
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
            try
            {
                // Navigate away
                var unlockedVaultViewModel = await VaultViewModel.UnlockAsync(unlockContract, IsReadOnly);
                NavigationRequested?.Invoke(this, new UnlockNavigationRequestedEventArgs(unlockedVaultViewModel, this));

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

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            await UnlockAsync(e.UnlockContract);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _connectionCts?.Cancel();
            _connectionCts?.Dispose();
            _connectionCts = null;
            IsConnected = false;
            LoginViewModel?.Dispose();
            NavigationRequested = null;
        }
    }
}
