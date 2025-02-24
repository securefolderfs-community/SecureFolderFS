using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IVaultService>, Inject<IVaultCredentialsService>]
    [Bindable(true)]
    public sealed partial class VaultPropertiesViewModel : BaseDesignationViewModel, IUnlockedViewContext, IAsyncInitialize
    {
        private readonly INavigator _innerNavigator;
        private readonly INavigator _outerNavigator;

        [ObservableProperty] private string? _SecurityText;
        [ObservableProperty] private string? _ContentCipherText;
        [ObservableProperty] private string? _FileNameCipherText;
        [ObservableProperty] private string? _ActiveFileSystemText;
        [ObservableProperty] private string? _FileSystemDescriptionText;

        /// <inheritdoc/>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel => UnlockedVaultViewModel.VaultViewModel;

        public VaultPropertiesViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator innerNavigator, INavigator outerNavigator)
        {
            ServiceProvider = DI.Default;
            UnlockedVaultViewModel = unlockedVaultViewModel;
            Title = "VaultProperties".ToLocalized();
            _innerNavigator = innerNavigator;
            _outerNavigator = outerNavigator;
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(UnlockedVaultViewModel.VaultViewModel.VaultModel.Folder, cancellationToken);
            ContentCipherText = string.IsNullOrEmpty(vaultOptions.ContentCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.ContentCipherId ?? "Unknown");
            FileNameCipherText = string.IsNullOrEmpty(vaultOptions.FileNameCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.FileNameCipherId ?? "Unknown") + $" + {vaultOptions.NameEncodingId}";
            ActiveFileSystemText = UnlockedVaultViewModel.StorageRoot.FileSystemName;
            FileSystemDescriptionText = UnlockedVaultViewModel.StorageRoot.Options.GetDescription();

            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeFirstAuthenticationAsync(CancellationToken cancellationToken)
        {
            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultViewModel.VaultModel, AuthenticationType.FirstStageOnly);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeSecondAuthenticationAsync(CancellationToken cancellationToken)
        {
            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultViewModel.VaultModel, AuthenticationType.ProceedingStageOnly);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ViewRecoveryAsync(CancellationToken cancellationToken)
        {
            using var previewRecoveryOverlay = new PreviewRecoveryOverlayViewModel(UnlockedVaultViewModel.VaultViewModel.VaultModel);

            await previewRecoveryOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(previewRecoveryOverlay);
        }

        [RelayCommand]
        private async Task ViewRecycleBinAsync(CancellationToken cancellationToken)
        {
            var recycleOverlay = new RecycleBinOverlayViewModel(UnlockedVaultViewModel, _outerNavigator);
            _ = recycleOverlay.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(recycleOverlay);
        }

        private async Task UpdateSecurityTextAsync(CancellationToken cancellationToken)
        {
            var items = await VaultCredentialsService.GetLoginAsync(UnlockedVaultViewModel.VaultViewModel.VaultModel.Folder, cancellationToken).ToArrayAsync(cancellationToken);
            SecurityText = string.Join(" + ", items.Select(x => x.Title));
        }
    }
}
