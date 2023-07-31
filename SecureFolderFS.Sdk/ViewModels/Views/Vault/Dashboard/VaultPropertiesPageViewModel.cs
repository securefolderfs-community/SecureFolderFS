using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    [Inject<IDialogService>]
    public sealed partial class VaultPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        [ObservableProperty] private string? _ContentCipherName;
        [ObservableProperty] private string? _FileNameCipherName;

        public VaultPropertiesPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService)
            : base(unlockedVaultViewModel, dashboardNavigationService)
        {
            ServiceProvider = Ioc.Default;
            var contentCipherId = unlockedVaultViewModel.UnlockedVaultModel.VaultInfoModel.ContentCipherId;
            var fileNameCipherId = unlockedVaultViewModel.UnlockedVaultModel.VaultInfoModel.FileNameCipherId;

            ContentCipherName = contentCipherId == string.Empty ? "None" : (contentCipherId ?? "Unknown");
            FileNameCipherName = fileNameCipherId == string.Empty ? "None" : (fileNameCipherId ?? "Unknown");
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ChangePasswordAsync()
        {
            //using var viewModel = new PasswordChangeDialogViewModel(UnlockedVaultViewModel.VaultViewModel.VaultModel);
            //await DialogService.ShowDialogAsync(viewModel);
        }
    }
}
