using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard;
using SecureFolderFS.Shared.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<INavigationService>(Visibility = "public", Name = "DashboardNavigationService")]
    public sealed partial class VaultDashboardPageViewModel : BaseVaultPageViewModel, IRecipient<VaultLockedMessage>
    {
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        public VaultDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService navigationService)
            : base(unlockedVaultViewModel.VaultViewModel, navigationService)
        {
            ServiceProvider = Ioc.Default;
            UnlockedVaultViewModel = unlockedVaultViewModel;
            DashboardNavigationService.NavigationChanged += DashboardNavigationService_NavigationChanged;

            WeakReferenceMessenger.Default.Register(this);
        }

        private void DashboardNavigationService_NavigationChanged(object? sender, IViewDesignation? e)
        {
            Title = (DashboardNavigationService.CurrentView as BaseDashboardPageViewModel)?.PageName;
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
