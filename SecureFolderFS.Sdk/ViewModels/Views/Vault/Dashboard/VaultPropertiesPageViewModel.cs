using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Dialogs;
using SecureFolderFS.Sdk.ViewModels.Vault;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    public sealed partial class VaultPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

        public VaultPropertiesPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService)
            : base(unlockedVaultViewModel, dashboardNavigationService)
        {
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        [RelayCommand]
        private async Task ChangePasswordAsync(CancellationToken cancellationToken)
        {
            await DialogService.ShowDialogAsync(new PasswordChangeDialogViewModel());
        }
    }
}
