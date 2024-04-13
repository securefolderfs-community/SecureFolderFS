using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Overlays;
using SecureFolderFS.Shared.EventArguments;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    [Inject<IOverlayService>]
    public sealed partial class VaultPropertiesViewModel : BaseDashboardViewModel
    {
        [ObservableProperty] private string? _ContentCipherName;
        [ObservableProperty] private string? _FileNameCipherName;

        /// <inheritdoc/>
        public override event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public VaultPropertiesViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
            : base(unlockedVaultViewModel)
        {
            ServiceProvider = Ioc.Default;
            Title = "Properties".ToLocalized();

            // TODO: Maybe add a method into one of the vault services to get the details about a vault
            ContentCipherName = "TODO";
            FileNameCipherName = "TODO";

            //var contentCipherId = unlockedVaultViewModel.VaultLifeTimeModel.VaultOptions.ContentCipherId;
            //var fileNameCipherId = unlockedVaultViewModel.VaultLifeTimeModel.VaultOptions.FileNameCipherId;

            //ContentCipherName = contentCipherId == string.Empty ? "NoEncryption".ToLocalized() : (contentCipherId ?? "Unknown");
            //FileNameCipherName = fileNameCipherId == string.Empty ? "NoEncryption".ToLocalized() : (fileNameCipherId ?? "Unknown");
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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
    }
}
