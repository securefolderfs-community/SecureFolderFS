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
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Shared.Extensions;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<IOverlayService>, Inject<IVaultService>, Inject<IVaultCredentialsService>, Inject<IIapService>]
    [Bindable(true)]
    public sealed partial class VaultPropertiesViewModel : BaseDesignationViewModel, IUnlockedViewContext, IAsyncInitialize, IDisposable
    {
        [ObservableProperty] private string? _SecurityText;
        [ObservableProperty] private string? _ContentCipherText;
        [ObservableProperty] private string? _FileNameCipherText;
        [ObservableProperty] private string? _FileNameShorteningText;
        [ObservableProperty] private string? _ActiveFileSystemText;
        [ObservableProperty] private string? _FileSystemDescriptionText;
        [ObservableProperty] private bool _IsAppPlatform;

        /// <inheritdoc/>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel => UnlockedVaultViewModel.VaultViewModel;

        /// <summary>
        /// Gets the recycle bin overlay view model.
        /// </summary>
        public RecycleBinOverlayViewModel RecycleBinOverlayViewModel { get; }

        public VaultPropertiesViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator innerNavigator, INavigator outerNavigator)
        {
            ServiceProvider = DI.Default;
            UnlockedVaultViewModel = unlockedVaultViewModel;
            RecycleBinOverlayViewModel = new(UnlockedVaultViewModel, outerNavigator);
            Title = "VaultProperties".ToLocalized();
        }

        /// <inheritdoc/>
        public async Task InitAsync(CancellationToken cancellationToken = default)
        {
            var vaultOptions = await VaultService.GetVaultOptionsAsync(UnlockedVaultViewModel.VaultFolder, cancellationToken);
            var areNamesEncrypted = !string.IsNullOrEmpty(vaultOptions.FileNameCipherId);
            var areContentsEncrypted = !string.IsNullOrEmpty(vaultOptions.ContentCipherId);

            ContentCipherText = !areContentsEncrypted ? "NoEncryption".ToLocalized() : (vaultOptions.ContentCipherId ?? "Unknown");
            FileNameCipherText = !areNamesEncrypted ? "NoEncryption".ToLocalized() : (vaultOptions.FileNameCipherId ?? "Unknown") + $" + {vaultOptions.NameEncodingId}";
            FileNameShorteningText = !areNamesEncrypted ? null : vaultOptions.ShorteningThreshold.ToString();
            ActiveFileSystemText = UnlockedVaultViewModel.StorageRoot.FileSystemName;
            FileSystemDescriptionText = UnlockedVaultViewModel.StorageRoot.Options.GetDescription();
            SecurityText = await VaultCredentialsService.FromUnlockProcedureAsync(UnlockedVaultViewModel.VaultFolder, vaultOptions.UnlockProcedure, cancellationToken);
            IsAppPlatform = vaultOptions.AppPlatform is not null;

            if (!RecycleBinOverlayViewModel.IsInitialized
                && (await IapService.IsOwnedAsync(IapProductType.Any, cancellationToken) || await RecycleBinOverlayViewModel.HasItemsAsync(cancellationToken)))
                await RecycleBinOverlayViewModel.InitAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeFirstAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (UnlockedVaultViewModel.Options.IsReadOnly
                || OverlayService.CurrentView is not null
                || IsAppPlatform)
                return;

            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultFolder, VaultViewModel.Title, AuthenticationStage.FirstStageOnly);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ChangeSecondAuthenticationAsync(CancellationToken cancellationToken)
        {
            if (UnlockedVaultViewModel.Options.IsReadOnly
                || OverlayService.CurrentView is not null
                || IsAppPlatform)
                return;

            using var credentialsOverlay = new CredentialsOverlayViewModel(UnlockedVaultViewModel.VaultFolder, VaultViewModel.Title, AuthenticationStage.ProceedingStageOnly);
            await credentialsOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(credentialsOverlay);
            await UpdateSecurityTextAsync(cancellationToken);
        }

        [RelayCommand]
        private async Task ViewRecoveryAsync(CancellationToken cancellationToken)
        {
            if (OverlayService.CurrentView is not null || IsAppPlatform)
                return;

            using var previewRecoveryOverlay = new PreviewRecoveryOverlayViewModel(UnlockedVaultViewModel.VaultFolder, VaultViewModel.Title);
            await previewRecoveryOverlay.InitAsync(cancellationToken);
            await OverlayService.ShowAsync(previewRecoveryOverlay);
        }

        [RelayCommand]
        private async Task ViewRecycleBinAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(100, cancellationToken);
            if (!await IapService.IsOwnedAsync(IapProductType.Any, cancellationToken))
            {
                // Users whose subscription expired must still be able to access items already
                // present in the recycle bin (the dialog opens with configuration locked).
                // The paywall is only shown when there is nothing to view
                if (!await RecycleBinOverlayViewModel.HasItemsAsync(cancellationToken))
                {
                    await OverlayService.ShowAsync(PaymentOverlayViewModel.Instance.WithInitAsync(cancellationToken));
                    if (!await IapService.IsOwnedAsync(IapProductType.Any, cancellationToken))
                        return;
                }
            }

            if (!RecycleBinOverlayViewModel.IsInitialized)
                await RecycleBinOverlayViewModel.InitAsync(cancellationToken);

            await OverlayService.ShowAsync(RecycleBinOverlayViewModel);
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
            var vaultOptions = await VaultService.GetVaultOptionsAsync(UnlockedVaultViewModel.VaultFolder, cancellationToken);
            SecurityText = await VaultCredentialsService.FromUnlockProcedureAsync(UnlockedVaultViewModel.VaultFolder, vaultOptions.UnlockProcedure, cancellationToken);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            RecycleBinOverlayViewModel.Dispose();
        }
    }
}
