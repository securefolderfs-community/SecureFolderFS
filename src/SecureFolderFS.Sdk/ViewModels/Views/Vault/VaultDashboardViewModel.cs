using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Bindable(true)]
    public sealed partial class VaultDashboardViewModel : BaseDashboardViewModel, IRecipient<VaultLockedMessage>
    {
        public INavigationService VaultNavigation { get; }
        
        public INavigationService DashboardNavigation { get; }

        public VaultDashboardViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService vaultNavigation, INavigationService dashboardNavigation)
            : base(unlockedVaultViewModel)
        {
            VaultNavigation = vaultNavigation;
            DashboardNavigation = dashboardNavigation;
            DashboardNavigation.NavigationChanged += DashboardNavigation_NavigationChanged;

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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
            await DashboardNavigation.TryNavigateAsync<VaultOverviewViewModel>();

            // TODO(n): Use GoBackAsync so the navigation is unified when more nested views are implemented
            // The current problem is that GoBackAsync does not update CurrentView
            // which the UI relies on when completing the animation
            //await DashboardNavigationService.GoBackAsync();
        }

        private void DashboardNavigation_NavigationChanged(object? sender, IViewDesignation? e)
        {
            Title = DashboardNavigation.CurrentView?.Title;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            DashboardNavigation.NavigationChanged -= DashboardNavigation_NavigationChanged;
            DashboardNavigation.Dispose();
        }
    }
}
