using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Messages;
using SecureFolderFS.Sdk.Models.Transitions;

namespace SecureFolderFS.Sdk.ViewModels.Pages.DashboardPages
{
    public sealed class VaultDashboardPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultDashboardPropertiesPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
            : base(messenger, vaultViewModel, VaultDashboardPageType.DashboardPropertiesPage)
        {
            base.NavigationItemViewModel = new()
            {
                Index = 1,
                NavigationAction = first => Messenger.Send(new DashboardNavigationRequestedMessage(VaultDashboardPageType.DashboardPropertiesPage, VaultViewModel) { Transition = new SuppressTransitionModel() }),
                SectionName = "Properties"
            };
        }
    }
}
