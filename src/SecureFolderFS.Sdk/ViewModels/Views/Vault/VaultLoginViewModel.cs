using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
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
    public sealed partial class VaultLoginViewModel : BaseVaultViewModel, INavigatable
    {
        [ObservableProperty] private LoginViewModel _LoginViewModel;

        public INavigator VaultNavigator { get; }

        /// <inheritdoc/>
        public event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public VaultLoginViewModel(IVaultModel vaultModel, INavigator vaultNavigator)
            : base(vaultModel)
        {
            ServiceProvider = Ioc.Default;
            Title = vaultModel.VaultName;
            VaultNavigator = vaultNavigator;
            _LoginViewModel = new(vaultModel, true);
            _LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return LoginViewModel.InitAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task BeginRecoveryAsync(CancellationToken cancellationToken)
        {
            var recoveryOverlay = new RecoveryOverlayViewModel(VaultModel.Folder);
            var result = await OverlayService.ShowAsync(recoveryOverlay);
            if (!result.Positive())
                return;

            if (recoveryOverlay.UnlockContract is null)
                return;

            await UnlockAsync(recoveryOverlay.UnlockContract);
        }

        private async Task UnlockAsync(IDisposable unlockContract)
        {
            try
            {
                // Create the storage layer
                var storageRoot = await VaultManagerService.CreateFileSystemAsync(VaultModel, unlockContract, default);

                // Update last access date
                await VaultModel.SetLastAccessDateAsync(DateTime.Now);

                // Notify that the vault has been unlocked
                WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

                // Navigate away
                var unlockedVaultViewModel = new UnlockedVaultViewModel(storageRoot, VaultModel);
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
        public override void Dispose()
        {
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            LoginViewModel.Dispose();
        }
    }
}
