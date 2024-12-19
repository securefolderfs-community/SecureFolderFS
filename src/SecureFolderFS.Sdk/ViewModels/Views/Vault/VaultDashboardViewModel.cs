using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Contexts;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared.ComponentModel;
using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Bindable(true)]
    public sealed partial class VaultDashboardViewModel : BaseDesignationViewModel, IRecipient<VaultLockedMessage>, IUnlockedViewContext, IDisposable
    {
        public INavigationService VaultNavigation { get; }
        
        public INavigationService DashboardNavigation { get; }

        /// <inheritdoc/>
        public UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        /// <inheritdoc/>
        public VaultViewModel VaultViewModel => UnlockedVaultViewModel.VaultViewModel;

        public VaultDashboardViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService vaultNavigation, INavigationService dashboardNavigation)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            VaultNavigation = vaultNavigation;
            DashboardNavigation = dashboardNavigation;
            DashboardNavigation.NavigationChanged += DashboardNavigation_NavigationChanged;

            WeakReferenceMessenger.Default.Register(this);
        }

        /// <inheritdoc/>
        public void Receive(VaultLockedMessage message)
        {
            // Free resources that are used by the dashboard
            if (UnlockedVaultViewModel.VaultViewModel.VaultModel.Equals(message.VaultModel))
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
        public void Dispose()
        {
            WeakReferenceMessenger.Default.UnregisterAll(this);
            DashboardNavigation.NavigationChanged -= DashboardNavigation_NavigationChanged;
            DashboardNavigation.Dispose();
        }
    }
}
