using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Shared.Utils;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy
{
    public sealed partial class LoginCredentialsViewModel : BaseLoginStrategyViewModel
    {
        private readonly IVaultUnlockingModel _vaultUnlockingModel;
        private readonly IVaultWatcherModel _vaultWatcherModel;
        private readonly IKeystoreModel _keystoreModel;
        private readonly IMessenger _messenger;

        public LoginCredentialsViewModel(IVaultUnlockingModel vaultUnlockingModel, IVaultWatcherModel vaultWatcherModel, IKeystoreModel keystoreModel, IMessenger messenger)
        {
            _vaultUnlockingModel = vaultUnlockingModel;
            _vaultWatcherModel = vaultWatcherModel;
            _keystoreModel = keystoreModel;
            _messenger = messenger;
        }

        [RelayCommand]
        private async Task UnlockVaultAsync(IPassword? password, CancellationToken cancellationToken)
        {
            if (password is null)
                return;

            IUnlockedVaultModel? unlockedVaultModel;
            using (_vaultWatcherModel.LockFolderAsync(cancellationToken))
            using (_vaultUnlockingModel)
            using (password)
            {
                var setFolderResult = await _vaultUnlockingModel.SetFolderAsync(_vaultWatcherModel.VaultModel.Folder, cancellationToken);
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

            var widgetsContextModel = new SavedWidgetsContextModel(_vaultWatcherModel.VaultModel); // TODO: Reuse the instance
            var vaultViewModel = new VaultViewModel(unlockedVaultModel, widgetsContextModel, _vaultWatcherModel.VaultModel);
            var dashboardPage = new VaultDashboardPageViewModel(vaultViewModel, _messenger);
            _ = dashboardPage.InitAsync(cancellationToken);

            WeakReferenceMessenger.Default.Send(new VaultUnlockedMessage(_vaultWatcherModel.VaultModel));
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(dashboardPage));
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            _keystoreModel.Dispose();
            _vaultUnlockingModel.Dispose();
        }
    }
}
