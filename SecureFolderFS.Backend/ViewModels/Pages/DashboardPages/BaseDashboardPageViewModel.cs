using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, ICleanable, IDisposable
    {
        protected IMessenger Messenger { get; }

        protected VaultViewModel VaultViewModel { get; }

        public VaultDashboardPageType VaultDashboardPageType { get; }

        public NavigationItemViewModel? NavigationItemViewModel { get; protected init; }

        protected BaseDashboardPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel, VaultDashboardPageType vaultDashboardPageType)
        {
            this.Messenger = messenger;
            this.VaultViewModel = vaultViewModel;
            this.VaultDashboardPageType = vaultDashboardPageType;
        }

        public virtual void Cleanup() { }

        public virtual void Dispose()
        {
            VaultViewModel.VaultInstance?.Dispose();
            VaultViewModel.VaultInstance = null;
        }
    }
}
