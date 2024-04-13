using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.EventArguments;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<ISettingsService>]
    public sealed partial class VaultLoginViewModel : BaseVaultViewModel
    {
        [ObservableProperty] private LoginControlViewModel _LoginViewModel;

        /// <inheritdoc/>
        public override event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public VaultLoginViewModel(IVaultModel vaultModel)
            : base(vaultModel)
        {
            ServiceProvider = Ioc.Default;
            Title = vaultModel.VaultName;
            _LoginViewModel = new(vaultModel, true);
            _LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return LoginViewModel.InitAsync(cancellationToken);
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            // Show vault tutorial
            if (SettingsService.AppSettings.ShouldShowVaultTutorial)
            {
                var explanationDialog = new ExplanationDialogViewModel();
                await explanationDialog.InitAsync();
                await OverlayService.ShowAsync(explanationDialog);

                SettingsService.AppSettings.ShouldShowVaultTutorial = false;
                await SettingsService.AppSettings.TrySaveAsync();
            }

            // Update last access date
            await VaultModel.SetLastAccessDateAsync(DateTime.Now);

            // Notify that the vault has been unlocked
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultModel));

            // Navigate away
            var unlockedVaultViewModel = new UnlockedVaultViewModel(e.StorageRoot, VaultModel);
            NavigationRequested?.Invoke(this, new UnlockNavigationRequestedEventArgs(unlockedVaultViewModel, this));

            // Dispose the current instance and navigate
            Dispose();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            LoginViewModel.Dispose();
        }
    }
}
