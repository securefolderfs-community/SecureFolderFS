using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Extensions;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Shared;
using SecureFolderFS.Shared.ComponentModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault
{
    [Inject<INavigationService>(Visibility = "public", Name = "DashboardNavigationService")]
    [Bindable(true)]
    public sealed partial class VaultDashboardViewModel : BaseDashboardViewModel, IRecipient<VaultLockedMessage>
    {
        public INavigator VaultNavigator { get; }

        public VaultDashboardViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigator vaultNavigator)
            : base(unlockedVaultViewModel)
        {
            ServiceProvider = DI.Default;
            VaultNavigator = vaultNavigator;
            DashboardNavigationService.NavigationChanged += DashboardNavigationService_NavigationChanged;

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
            if (VaultModel.Equals(message.VaultModel))
                Dispose();
        }

        [RelayCommand]
        private async Task GoBackAsync()
        {
            await DashboardNavigationService.TryNavigateAsync<VaultOverviewViewModel>();

            // TODO(n): Use GoBackAsync so the navigation is unified when more nested views are implemented
            // The current problem is that GoBackAsync does not update CurrentView
            // which the UI relies on when completing the animation
            //await DashboardNavigationService.GoBackAsync();
        }

        private void DashboardNavigationService_NavigationChanged(object? sender, IViewDesignation? e)
        {
            Title = DashboardNavigationService.CurrentView?.Title;
        }

        /// <inheritdoc/>
        public override void Dispose()
        {
            DashboardNavigationService.NavigationChanged -= DashboardNavigationService_NavigationChanged;
            DashboardNavigationService.Dispose();
        }
    }
}
