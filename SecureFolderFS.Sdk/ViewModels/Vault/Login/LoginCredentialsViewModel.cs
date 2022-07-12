using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Vault.Login
{
    public sealed class LoginCredentialsViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly IMessenger _messenger;
        private readonly IVaultModel _vaultModel;
        private readonly IUnlockingModel<IUnlockedVaultModel> _unlockingModel;

        public IAsyncRelayCommand UnlockVaultCommand { get; }

        public LoginCredentialsViewModel(IMessenger messenger, IVaultModel vaultModel, IUnlockingModel<IUnlockedVaultModel> unlockingModel)
        {
            _messenger = messenger;
            _vaultModel = vaultModel;
            _unlockingModel = unlockingModel;
            UnlockVaultCommand = new AsyncRelayCommand<IPassword?>(UnlockVaultAsync);
        }

        private async Task UnlockVaultAsync(IPassword? password, CancellationToken cancellationToken = default)
        {
            if (password is null)
                return;

            // Check if the folder is accessible
            if (!await _vaultModel.IsAccessibleAsync())
                return; // TODO: Report the issue

            // Unlock the vault
            var unlockedVaultModel = await _unlockingModel.UnlockAsync(password, cancellationToken);
            if (unlockedVaultModel is null)
                return; // TODO: Report the issue

            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(new VaultDashboardPageViewModel(unlockedVaultModel, _vaultModel, _messenger)));
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Initialize the unlocking model
            await _unlockingModel.InitAsync(cancellationToken); // TODO: This should be called only once!

            // Check if the vault is supported
            if (!await _unlockingModel.IsSupportedAsync(cancellationToken))
                return; // TODO: Report the issue
        }
    }
}
