using System;
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
using SecureFolderFS.Shared.Extensions;

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
            var vaultOptions = await VaultService.GetVaultOptionsAsync(UnlockedVaultViewModel.VaultFolder, cancellationToken);
            ContentCipherText = string.IsNullOrEmpty(vaultOptions.ContentCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.ContentCipherId ?? "Unknown");
            FileNameCipherText = string.IsNullOrEmpty(vaultOptions.FileNameCipherId) ? "NoEncryption".ToLocalized() : (vaultOptions.FileNameCipherId ?? "Unknown") + $" + {vaultOptions.NameEncodingId}";
            ActiveFileSystemText = UnlockedVaultViewModel.StorageRoot.FileSystemName;
            FileSystemDescriptionText = UnlockedVaultViewModel.StorageRoot.Options.GetDescription();

            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeFirstAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (UnlockedVaultViewModel.Options.IsReadOnly)
                return;

            if (OverlayService.CurrentView is not null)
                return;

            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultFolder, VaultViewModel.Title, AuthenticationStage.FirstStageOnly);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeSecondAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (UnlockedVaultViewModel.Options.IsReadOnly)
                return;

            if (OverlayService.CurrentView is not null)
                return;

            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultFolder, VaultViewModel.Title, AuthenticationStage.ProceedingStageOnly);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ViewRecoveryAsync(CancellationToken cancellationToken)
        {
            if (OverlayService.CurrentView is not null)
                return;

            using var previewRecoveryOverlay = new PreviewRecoveryOverlayViewModel(UnlockedVaultViewModel.VaultFolder, VaultViewModel.Title);
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

        [RelayCommand]
        private void ToggleFileSystemText()
        {
            if (string.IsNullOrEmpty(FileSystemDescriptionText))
                return;

            (ActiveFileSystemText, FileSystemDescriptionText) = (FileSystemDescriptionText, ActiveFileSystemText);
        }

        private async Task UpdateSecurityTextAsync(CancellationToken cancellationToken)
        {
            try
            {
                var loginItems = await VaultCredentialsService
                    .GetLoginAsync(UnlockedVaultViewModel.VaultFolder, cancellationToken)
                    .ToArrayAsyncImpl(cancellationToken);
                SecurityText = string.Join(" + ", loginItems.Select(x => x.Title));
            }
            catch (Exception)
            {
                SecurityText = "AuthenticationUnavailable".ToLocalized();
            }
        }
    }
}
