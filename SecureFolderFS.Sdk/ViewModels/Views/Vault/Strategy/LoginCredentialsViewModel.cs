using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy
{
    [Inject<IDialogService>, Inject<ISettingsService>]
    public sealed partial class LoginCredentialsViewModel : ObservableObject, IDisposable
    {
        private readonly VaultViewModel _vaultViewModel;
        private readonly IKeystoreModel _keystoreModel;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private readonly IVaultUnlockingModel _vaultUnlockingModel;
        private readonly INavigationService _navigationService;

        [ObservableProperty] private bool _IsInvalidPasswordShown;

        // TODO: Reduce number of parameters
        public LoginCredentialsViewModel(VaultViewModel vaultViewModel, IKeystoreModel keystoreModel, IVaultWatcherModel vaultWatcherModel, IVaultUnlockingModel vaultUnlockingModel, INavigationService navigationService)
        {
            ServiceProvider = Ioc.Default;
            _vaultViewModel = vaultViewModel;
            _keystoreModel = keystoreModel;
            _vaultWatcherModel = vaultWatcherModel;
            _vaultUnlockingModel = vaultUnlockingModel;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task UnlockVaultAsync(IPassword? password, CancellationToken cancellationToken)
        {
            if (password is null)
                return;

            IUnlockedVaultModel? unlockedVaultModel;
            using (await _vaultWatcherModel.LockFolderAsync(cancellationToken))
            using (_vaultUnlockingModel)
            using (password)
            {
                var setFolderResult = await _vaultUnlockingModel.SetFolderAsync(_vaultViewModel.VaultModel.Folder, cancellationToken);
                if (!setFolderResult.Successful)
                    return; // TODO: Report the issue

                var setKeystoreResult = await _vaultUnlockingModel.SetKeystoreAsync(_keystoreModel, cancellationToken);
                if (!setKeystoreResult.Successful)
                    return; // TODO: Report the issue

                var unlockResult = await _vaultUnlockingModel.UnlockAsync(password, cancellationToken);
                if (!unlockResult.Successful)
                {
                    IsInvalidPasswordShown = true;
                    return; // TODO: Report the issue (The issue might be different than invalid password)
                }

                unlockedVaultModel = unlockResult.Value;

                // Don't forget to dispose the keystore after it's been used
                _keystoreModel.Dispose();
            }

            if (unlockedVaultModel is null)
                throw new InvalidOperationException($"Invalid state. {nameof(unlockedVaultModel)} shouldn't be null.");

            // Update last access date
            await _vaultViewModel.VaultModel.SetLastAccessDateAsync(DateTime.Now, cancellationToken);

            var unlockedVaultViewModel = new UnlockedVaultViewModel(_vaultViewModel, unlockedVaultModel);
            var dashboardPage = new VaultDashboardPageViewModel(unlockedVaultViewModel, _navigationService);

            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(_vaultViewModel.VaultModel));
            await _navigationService.TryNavigateAndForgetAsync(dashboardPage);

            if (!SettingsService.AppSettings.WasVaultFolderExplanationShown)
            {
                var explanationDialog = new ExplanationDialogViewModel();
                await explanationDialog.InitAsync(cancellationToken);
                await DialogService.ShowDialogAsync(explanationDialog);

                SettingsService.AppSettings.WasVaultFolderExplanationShown = true;
                await SettingsService.AppSettings.TrySaveAsync(cancellationToken);
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keystoreModel.Dispose();
            _vaultUnlockingModel.Dispose();
        }
    }
}
