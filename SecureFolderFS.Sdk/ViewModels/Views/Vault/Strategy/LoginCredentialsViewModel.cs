using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Strategy
{
    public sealed partial class LoginCredentialsViewModel : ObservableObject, IDisposable
    {
        private readonly VaultViewModel _vaultViewModel;
        private readonly IKeystoreModel _keystoreModel;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private readonly IVaultUnlockingModel _vaultUnlockingModel;

        public LoginCredentialsViewModel(VaultViewModel vaultViewModel, IVaultUnlockingModel vaultUnlockingModel, IVaultWatcherModel vaultWatcherModel, IKeystoreModel keystoreModel)
        {
            _vaultViewModel = vaultViewModel;
            _vaultUnlockingModel = vaultUnlockingModel;
            _vaultWatcherModel = vaultWatcherModel;
            _keystoreModel = keystoreModel;
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
                    return; // TODO: Report the issue

                unlockedVaultModel = unlockResult.Value;

                // Don't forget to dispose the keystore after it's been used
                _keystoreModel.Dispose();
            }

            if (unlockedVaultModel is null)
                throw new InvalidOperationException($"Invalid state. {nameof(unlockedVaultModel)} shouldn't be null.");

            // Update last access date
            await _vaultViewModel.VaultModel.SetLastAccessDateAsync(DateTime.Now, cancellationToken);

            var unlockedVaultViewModel = new UnlockedVaultViewModel(_vaultViewModel, unlockedVaultModel);
            var dashboardPage = new VaultDashboardPageViewModel(unlockedVaultViewModel);
            _ = dashboardPage.InitAsync(cancellationToken);

            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(_vaultViewModel.VaultModel));
            WeakReferenceMessenger.Default.Send(new NavigationMessage(dashboardPage));
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _keystoreModel.Dispose();
            _vaultUnlockingModel.Dispose();
        }
    }
}
