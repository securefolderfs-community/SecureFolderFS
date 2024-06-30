using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Credentials;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IVaultService>]
    [Bindable(true)]
    public sealed partial class VaultPropertiesViewModel : BaseDashboardViewModel
    {
        [ObservableProperty] private string? _ContentCipherText;
        [ObservableProperty] private string? _FileNameCipherText;
        [ObservableProperty] private string? _SecurityText;

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
            ContentCipherText = string.IsNullOrEmpty(vaultOptions.ContentCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.ContentCipherId ?? "Unknown");
            FileNameCipherText = string.IsNullOrEmpty(vaultOptions.FileNameCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.FileNameCipherId ?? "Unknown");

            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeFirstAuthenticationAsync(CancellationToken cancellationToken)
        {
            var item = await VaultService.GetLoginAsync(UnlockedVaultViewModel.VaultModel.Folder, cancellationToken).FirstOrDefaultAsync(cancellationToken);
            if (item is null)
                return;

            var selectionViewModel = new CredentialsSelectionViewModel(UnlockedVaultViewModel.VaultModel.Folder, item);
            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultModel, selectionViewModel);

            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeSecondAuthenticationAsync(CancellationToken cancellationToken)
        {
            var item = await VaultService.GetLoginAsync(UnlockedVaultViewModel.VaultModel.Folder, cancellationToken).ElementAtOrDefaultAsync(1, cancellationToken);
            var selectionViewModel = item is not null
                ? new CredentialsSelectionViewModel(UnlockedVaultViewModel.VaultModel.Folder, item)
                : new CredentialsSelectionViewModel(UnlockedVaultViewModel.VaultModel.Folder, AuthenticationType.ProceedingStageOnly);

            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultModel, selectionViewModel);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ViewRecoveryAsync(CancellationToken cancellationToken)
        {
            using var previewRecoveryOverlay = new PreviewRecoveryOverlayViewModel(UnlockedVaultViewModel.VaultModel);

            await previewRecoveryOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(previewRecoveryOverlay);
        }

        private async Task UpdateSecurityTextAsync(CancellationToken cancellationToken)
        {
            var items = await VaultService.GetLoginAsync(UnlockedVaultViewModel.VaultModel.Folder, cancellationToken).ToArrayAsync(cancellationToken);
            SecurityText = string.Join(" + ", items.Select(x => x.DisplayName));
        }
    }
}
