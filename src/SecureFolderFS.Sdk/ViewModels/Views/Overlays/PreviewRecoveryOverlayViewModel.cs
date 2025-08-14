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
using OwlCore.Storage;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class PreviewRecoveryOverlayViewModel : OverlayViewModel, IAsyncInitialize, IDisposable
    {
        private readonly string? _vaultName;
        private readonly IFolder _vaultFolder;
        private readonly LoginViewModel _loginViewModel;
        private readonly RecoveryPreviewControlViewModel _recoveryViewModel;

        [ObservableProperty] private INotifyPropertyChanged? _CurrentViewModel;

        public PreviewRecoveryOverlayViewModel(IFolder vaultFolder, string? vaultName)
        {
            ServiceProvider = DI.Default;
            _vaultFolder = vaultFolder;
            _vaultName = vaultName;
            _loginViewModel = new(_vaultFolder, LoginViewType.Basic) { Title = vaultName };
            _recoveryViewModel = new();

            CurrentViewModel = _loginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryText = "Continue".ToLocalized();

            _loginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return _loginViewModel.InitAsync(cancellationToken);
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultFolder);
            using (e.UnlockContract)
            {
                // Prepare the recovery view
                _recoveryViewModel.VaultId = vaultOptions.VaultId;
                _recoveryViewModel.Title = _vaultName;
                _recoveryViewModel.RecoveryKey = e.UnlockContract.ToString();

                // Change view to recovery
                CurrentViewModel = _recoveryViewModel;

                // Adjust the overlay
                PrimaryText = null;
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
