using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.ViewModels.Dashboard.Navigation;
using SecureFolderFS.Shared.Utils;

namespace SecureFolderFS.Sdk.ViewModels.Pages.DashboardPages
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, ICleanable, IDisposable
    {
        protected IMessenger Messenger { get; }

        protected VaultViewModelDeprecated VaultViewModel { get; }

        public VaultDashboardPageType VaultDashboardPageType { get; }

        public NavigationItemViewModel? NavigationItemViewModel { get; protected init; }

        protected BaseDashboardPageViewModel(IMessenger messenger, VaultViewModelDeprecated vaultViewModel, VaultDashboardPageType vaultDashboardPageType)
        {
            Messenger = messenger;
            VaultViewModel = vaultViewModel;
            VaultDashboardPageType = vaultDashboardPageType;
        }

        public virtual void Cleanup() { }

        public virtual void Dispose()
        {
            VaultViewModel.VaultInstance?.Dispose();
            VaultViewModel.VaultInstance = null;
        }
    }
}
