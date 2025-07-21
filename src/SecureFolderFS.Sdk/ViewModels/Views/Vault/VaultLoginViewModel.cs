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
        [ObservableProperty] private bool _IsReadOnly;
        [ObservableProperty] private LoginViewModel _LoginViewModel;

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
            _LoginViewModel = new(vaultViewModel.VaultModel, LoginViewType.Full);
            _LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await LoginViewModel.InitAsync(cancellationToken);

            // Test for quick unlock on mobile
            var recoveryKey = VaultViewModel.Title switch
            {
                "Vault V3" => "nn9oKIELbkAl3XevD/dhVhnBQcfkDA5wfLnY+aAUoK8=@@@w3jNIbmsDwThbNkuGqpVdCvCiU7RQHQtkEqGfBPqDRc=",
                "Plaintext Vault" => "lZbz5sWmeYDyyebm3LgPmvApNPsiyphj6zW4YZ2NuG8=@@@mEp3pOlUSXr0Yr47B+Se4M3ZXN8wPU/BlgFkLSpULiQ=",
                "TestFolder" => "AT690eDQi71F2pvBMOTW9tYaavdZxI5hF+in8GgVW+U=@@@zIMim+84MPoubryk2Ne9A8S5cXE18MOzLyET7yI6p/E=",
                _ => null
            };

            if (recoveryKey is null)
                return;

            var unlockContract = await VaultManagerService.RecoverAsync(VaultViewModel.VaultModel.Folder, recoveryKey, cancellationToken);
            await Task.Delay(200);
            await UnlockAsync(unlockContract);
        }

        [RelayCommand]
        private async Task BeginRecoveryAsync(CancellationToken cancellationToken)
        {
            var recoveryOverlay = new RecoveryOverlayViewModel(VaultViewModel.VaultModel.Folder);
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
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            LoginViewModel.Dispose();
        }
    }
}
