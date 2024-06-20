using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IVaultService>]
    public sealed partial class VaultPropertiesViewModel : BaseDashboardViewModel
    {
        [ObservableProperty] private string? _ContentCipherName;
        [ObservableProperty] private string? _FileNameCipherName;

        public VaultPropertiesViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
            : base(unlockedVaultViewModel)
        {
            ServiceProvider = Ioc.Default;
            Title = "VaultProperties".ToLocalized();
        }

        /// <inheritdoc/>
        public override async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(UnlockedVaultViewModel.VaultModel.Folder, cancellationToken);
            ContentCipherName = string.IsNullOrEmpty(vaultOptions.ContentCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.ContentCipherId ?? "Unknown");
            FileNameCipherName = string.IsNullOrEmpty(vaultOptions.FileNameCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.FileNameCipherId ?? "Unknown");
        }

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            var viewModel = new PasswordChangeDialogViewModel(UnlockedVaultViewModel.VaultModel);
            await OverlayService.ShowAsync(viewModel);
        }

        [RelayCommand]
        private async Task ChangeAuthenticationAsync()
        {
            // The dialog would have to have a common control for providing credentials which would be shared between the dialog and login screen
        }

        [RelayCommand]
        private async Task ViewRecoveryAsync(CancellationToken cancellationToken)
        {
            var recoveryOverlay = new PreviewRecoveryOverlayViewModel(UnlockedVaultViewModel.VaultModel);
            _ = recoveryOverlay.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(recoveryOverlay);
        }
    }
}
