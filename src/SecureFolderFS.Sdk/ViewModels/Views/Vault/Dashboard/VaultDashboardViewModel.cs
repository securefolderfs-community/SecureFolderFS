using CommunityToolkit.Mvvm.DependencyInjection;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.EventArguments;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;
using SecureFolderFS.Shared.ComponentModel;
using SecureFolderFS.Shared.EventArguments;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    [Inject<INavigationService>(Visibility = "public", Name = "DashboardNavigationService")]
    public sealed partial class VaultDashboardViewModel : BaseDashboardViewModel, IRecipient<VaultLockedMessage>
    {
        /// <inheritdoc/>
        public override event EventHandler<NavigationRequestedEventArgs>? NavigationRequested;

        public VaultDashboardViewModel(UnlockedVaultViewModel unlockedVaultViewModel)
            : base(unlockedVaultViewModel)
        {
            ServiceProvider = Ioc.Default;
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
        private void GoBack()
        {
            NavigationRequested?.Invoke(this, new BackNavigationRequestedEventArgs(this));
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
