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
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<ISettingsService>, Inject<IVaultManagerService>]
    [Bindable(true)]
    public sealed partial class VaultLoginViewModel : BaseDesignationViewModel, IVaultViewContext, INavigatable, IAsyncInitialize, IDisposable
    {
        private readonly IRemoteResource<IFolder>? _remoteVault;
        private IFolder? _vaultFolder;

        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private LoginViewModel? _LoginViewModel;

        public INavigationService VaultNavigation { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel { get; }

        /// <inheritdoc/>
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public VaultLoginViewModel(IFolder vaultFolder, VaultViewModel vaultViewModel, INavigationService vaultNavigation)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            Title = vaultViewModel.Title;
            VaultNavigation = vaultNavigation;
            VaultViewModel = vaultViewModel;
            LoginViewModel = new(_vaultFolder, LoginViewType.Full) { Title = vaultViewModel.Title };
            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        public VaultLoginViewModel(IRemoteResource<IFolder> remoteVault, VaultViewModel vaultViewModel, INavigationService vaultNavigation)
        {
            ServiceProvider = DI.Default;
            Title = vaultViewModel.Title;
            VaultNavigation = vaultNavigation;
            VaultViewModel = vaultViewModel;
            _remoteVault = remoteVault;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(LoginViewModel);
            await LoginViewModel.InitAsync(cancellationToken);

            #region Test for quick unlock on mobile

            if (_vaultFolder is null)
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

            var unlockContract = await VaultManagerService.RecoverAsync(_vaultFolder, recoveryKey, cancellationToken);
            await Task.Delay(200);
            await UnlockAsync(unlockContract);

            #endregion
        }

        [RelayCommand]
        private async Task ConnectToVaultAsync(CancellationToken cancellationToken)
        {
            if (_remoteVault is null)
                return;

            LoginViewModel?.Dispose();
            _vaultFolder = await _remoteVault.ConnectAsync(cancellationToken);
            LoginViewModel = new(_vaultFolder, LoginViewType.Full) { Title = VaultViewModel.Title };
            await LoginViewModel.InitAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task BeginRecoveryAsync(CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(_vaultFolder);

            var recoveryOverlay = new RecoveryOverlayViewModel(_vaultFolder);
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
            LoginViewModel?.Dispose();
        }
    }
}
