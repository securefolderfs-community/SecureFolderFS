using System;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Messages.Navigation;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Storage;
using SecureFolderFS.Sdk.ViewModels.Pages.Vault;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Vault.LoginStrategy
{
    public sealed partial class LoginCredentialsViewModel : ObservableObject, IAsyncInitialize
    {
        private readonly IAsyncValidator<IFolder> _vaultValidator;
        private readonly IVaultUnlockingModel _vaultUnlockingModel;
        private readonly IKeystoreModel _keystoreModel;
        private readonly IVaultModel _vaultModel;
        private readonly IMessenger _messenger;

        private IVaultService VaultService { get; } = Ioc.Default.GetRequiredService<IVaultService>();

        public LoginCredentialsViewModel(IVaultUnlockingModel vaultUnlockingModel, IKeystoreModel keystoreModel, IVaultModel vaultModel, IMessenger messenger)
        {
            _vaultUnlockingModel = vaultUnlockingModel;
            _keystoreModel = keystoreModel;
            _vaultModel = vaultModel;
            _messenger = messenger;
            _vaultValidator = VaultService.GetVaultValidator();
        }

        [RelayCommand]
        private async Task UnlockVaultAsync(IPassword? password, CancellationToken cancellationToken)
        {
            if (password is null)
                return;

            // Check if the folder is accessible
            if (!await _vaultModel.IsAccessibleAsync(cancellationToken))
                return; // TODO: Report the issue

            IUnlockedVaultModel? unlockedVaultModel;

            _ = await _vaultModel.LockFolderAsync(cancellationToken);
            using (_vaultModel.FolderLock)
            using (_vaultUnlockingModel)
            using (password)
            {
                var setFolderResult = await _vaultUnlockingModel.SetFolderAsync(_vaultModel.Folder, cancellationToken);
                if (!setFolderResult.IsSuccess)
                    return; // TODO: Report the issue

                var setKeystoreResult = await _vaultUnlockingModel.SetKeystoreAsync(_keystoreModel, cancellationToken);
                if (!setKeystoreResult.IsSuccess)
                    return; // TODO: Report the issue

                var unlockResult = await _vaultUnlockingModel.UnlockAsync(password, cancellationToken);
                if (!unlockResult.IsSuccess)
                    return; // TODO: Report the issue

                unlockedVaultModel = unlockResult.Value;

                // Don't forget to dispose the keystore after its been used
                _keystoreModel.Dispose();
            }

            if (unlockedVaultModel is null)
                throw new InvalidOperationException($"Invalid state. {nameof(unlockedVaultModel)} shouldn't be null.");

            var vaultViewModel = new VaultViewModel(unlockedVaultModel, null, _vaultModel);
            var dashboardPage = new VaultDashboardPageViewModel(vaultViewModel, _messenger);
            _ = dashboardPage.InitAsync(cancellationToken);
            
            WeakReferenceMessenger.Default.Send(new NavigationRequestedMessage(dashboardPage));
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            // Check if the vault is supported
            var vaultValidationResult = await _vaultValidator.ValidateAsync(_vaultModel.Folder, cancellationToken);
            if (!vaultValidationResult.IsSuccess)
                return; // TODO: Report the issue
        }
    }
}
