using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Controls.Authentication;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SecureFolderFS.Sdk.ViewModels.Views.Overlays
{
    [Inject<IVaultService>]
    public sealed partial class PreviewRecoveryOverlayViewModel : OverlayViewModel, IAsyncInitialize
    {
        private readonly IVaultModel _vaultModel;

        [ObservableProperty] private LoginControlViewModel _LoginViewModel;
        [ObservableProperty] private RecoveryPreviewControlViewModel _RecoveryViewModel;
        [ObservableProperty] private INotifyPropertyChanged? _CurrentViewModel;
        [ObservableProperty] private ICommand? _ContinueCommand;

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
            LoginViewModel.PropertyChanged += LoginViewModel_PropertyChanged;
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

        private async void LoginViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoginControlViewModel.CurrentViewModel)
                && LoginViewModel.CurrentViewModel is AuthenticationViewModel authenticationViewModel)
            {
                ContinueCommand = authenticationViewModel.ProvideCredentialsCommand;
            }
        }

        private async void LoginViewModel_VaultUnlocked(object? sender, VaultUnlockedEventArgs e)
        {
            var vaultId = await VaultService.GetVaultIdAsync(_vaultModel.Folder);
            using (e.UnlockContract)
            {
                // Prepare the recovery view
                RecoveryViewModel.VaultId = vaultId;
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
