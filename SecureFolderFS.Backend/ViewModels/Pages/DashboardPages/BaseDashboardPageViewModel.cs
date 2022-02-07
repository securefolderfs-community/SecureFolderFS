using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Utils;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, ICleanable
    {
        protected IMessenger Messenger { get; }

        protected UnlockedVaultModel UnlockedVaultModel { get; }

        public VaultDashboardPageType VaultDashboardPageType { get; }

        public NavigationItemViewModel? NavigationItemViewModel { get; protected init; }

        protected BaseDashboardPageViewModel(IMessenger messenger, UnlockedVaultModel unlockedVaultModel, VaultDashboardPageType vaultDashboardPageType)
        {
            this.Messenger = messenger;
            this.UnlockedVaultModel = unlockedVaultModel;
            this.VaultDashboardPageType = vaultDashboardPageType;
        }

        public virtual void Cleanup() { }
    }
}
