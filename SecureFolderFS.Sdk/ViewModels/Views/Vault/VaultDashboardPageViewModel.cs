using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    public sealed partial class VaultDashboardPageViewModel : BaseVaultPageViewModel, IRecipient<VaultLockedMessage>
    {
        public INavigationService DashboardNavigationService { get; } = Ioc.Default.GetRequiredService<INavigationService>();

        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        public VaultDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService navigationService)
            : base(unlockedVaultViewModel.VaultViewModel, navigationService)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            // Free resources that are used by the dashboard
            if (VaultViewModel.VaultModel.Equals(message.VaultModel))
                Dispose();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await DashboardNavigationService.TryNavigateAsync<VaultOverviewPageViewModel>();
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            DashboardNavigationService.Dispose();
        }
    }
}
