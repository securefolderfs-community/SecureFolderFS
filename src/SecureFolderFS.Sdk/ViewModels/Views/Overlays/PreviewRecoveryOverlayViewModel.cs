using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IVaultService>]
    public sealed partial class PreviewRecoveryOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        private readonly IVaultModel _vaultModel;

        [ObservableProperty] private LoginControlViewModel _LoginViewModel;
        [ObservableProperty] private RecoveryPreviewControlViewModel _RecoveryViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _CurrentViewModel;

        public PreviewRecoveryOverlayViewModel(IVaultModel vaultModel)
        {
            ServiceProvider = Ioc.Default;
            _vaultModel = vaultModel;
            LoginViewModel = new(_vaultModel, false);
            RecoveryViewModel = new();
            
            CurrentViewModel = LoginViewModel;
            Title = "Authenticate".ToLocalized();
            PrimaryButtonText = "Continue".ToLocalized();
            CloseButtonText = "Close".ToLocalized();

            LoginViewModel.VaultUnlocked += LoginViewModel_VaultUnlocked;
        }

        /// <inheritdoc/>
        public Task InitAsync(CancellationToken cancellationToken = default)
        {
            return LoginViewModel.InitAsync(cancellationToken);
        }

        /// <inheritdoc/>
        public override void OnDisappearing()
        {
            LoginViewModel.Dispose();
            RecoveryViewModel.MasterKey = null;
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(_vaultModel.Folder);
            using (e.UnlockContract)
            {
                // Prepare the recovery view
                RecoveryViewModel.VaultId = vaultOptions.VaultId;
                RecoveryViewModel.VaultName = _vaultModel.VaultName;
                RecoveryViewModel.MasterKey = e.UnlockContract.ToString();

                // Change view to recovery
                CurrentViewModel = RecoveryViewModel;

                // Adjust the overlay
                PrimaryButtonText = null;
                Title = "VaultRecovery".ToLocalized();
            }
        }
    }
}
