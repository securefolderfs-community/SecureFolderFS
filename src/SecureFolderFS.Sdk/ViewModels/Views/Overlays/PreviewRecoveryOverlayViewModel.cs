using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class PreviewRecoveryOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly IVaultModel _vaultModel;
        private readonly LoginViewModel _loginViewModel;
        private readonly RecoveryPreviewControlViewModel _recoveryViewModel;

        [ObservableProperty] private INotifyPropertyChanged? _CurrentViewModel;

        public PreviewRecoveryOverlayViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = DI.Default;
            _vaultModel = vaultModel;
            _loginViewModel = new(_vaultModel, LoginViewType.Basic);
            _recoveryViewModel = new();

            CurrentViewModel = _loginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Continue".ToLocalized();

            _loginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return _loginViewModel.InitAsync(cancellationToken);
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultModel.Folder);
            using (e.UnlockContract)
            {
                // Prepare the recovery view
                _recoveryViewModel.VaultId = vaultOptions.VaultId;
                _recoveryViewModel.Title = _vaultModel.VaultName;
                _recoveryViewModel.RecoveryKey = e.UnlockContract.ToString();

                // Change view to recovery
                CurrentViewModel = _recoveryViewModel;

                // Adjust the overlay
                PrimaryButtonText = null;
                Title = "VaultRecovery".ToLocalized();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            _recoveryViewModel.RecoveryKey = null;
            _loginViewModel.Dispose();
        }
    }
}
