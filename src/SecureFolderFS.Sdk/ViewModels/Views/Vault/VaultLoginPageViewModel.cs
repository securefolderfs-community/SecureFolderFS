using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Extensions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IDialogService>, Inject<ISettingsService>]
    public sealed partial class VaultLoginPageViewModel : BaseVaultPageViewModel
    {
        [ObservableProperty] private string? _VaultName;
        [ObservableProperty] private LoginViewModel _LoginViewModel;

        public VaultLoginPageViewModel(VaultViewModel vaultViewModel, INavigationService navigationService)
            : base(vaultViewModel, navigationService)
        {
            ServiceProvider = Ioc.Default;
            VaultName = vaultViewModel.VaultModel.VaultName;
            _LoginViewModel = new(vaultViewModel.VaultModel, true);
            _LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            await LoginViewModel.InitAsync(cancellationToken);
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            if (!SettingsService.AppSettings.WasVaultFolderExplanationShown)
            {
                var explanationDialog = new ExplanationDialogViewModel();
                await explanationDialog.InitAsync();
                await DialogService.ShowDialogAsync(explanationDialog);

                SettingsService.AppSettings.WasVaultFolderExplanationShown = true;
                await SettingsService.AppSettings.TrySaveAsync();
            }

            // Update last access date
            await VaultViewModel.VaultModel.SetLastAccessDateAsync(DateTime.Now);

            // Create view models
            var unlockedVaultViewModel = new UnlockedVaultViewModel(VaultViewModel, e.VaultLifecycle);
            var dashboardPage = new VaultDashboardPageViewModel(unlockedVaultViewModel, NavigationService);

            // Notify that the vault has been unlocked
            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(VaultViewModel.VaultModel));

            // Dispose the current instance and navigate
            Dispose();
            await NavigationService.TryNavigateAndForgetAsync(dashboardPage);
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            LoginViewModel.VaultUnlocked -= LoginViewModel_VaultUnlocked;
            LoginViewModel.Dispose();
        }
    }
}
