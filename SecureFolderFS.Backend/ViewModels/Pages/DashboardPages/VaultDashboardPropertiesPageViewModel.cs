using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public sealed class VaultDashboardPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public override int Index { get; }

        public override Action<DashboardNavigationItemViewModel?> NavigationAction { get; }

        public override string SectionName { get; }

        public VaultDashboardPropertiesPageViewModel(UnlockedVaultModel unlockedVaultModel)
            : base(unlockedVaultModel)
        {
            this.Index = 1;
            this.NavigationAction = (first) => WeakReferenceMessenger.Default.Send(new DashboardNavigationRequestedMessage(VaultDashboardPageType.DashboardPropertiesPage, unlockedVaultModel) { From = first?.SectionName});
            this.SectionName = "Properties";
        }
    }
}
