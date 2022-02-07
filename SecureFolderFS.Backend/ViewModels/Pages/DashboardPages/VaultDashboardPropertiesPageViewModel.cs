using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Models.Transitions;

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public sealed class VaultDashboardPropertiesPageViewModel : BaseDashboardPageViewModel
    {
        public VaultDashboardPropertiesPageViewModel(IMessenger messenger, UnlockedVaultModel unlockedVaultModel)
            : base(messenger, unlockedVaultModel, VaultDashboardPageType.DashboardPropertiesPage)
        {
            base.NavigationItemViewModel = new()
            {
                Index = 1,
                NavigationAction = (first) => Messenger.Send(new DashboardNavigationRequestedMessage(VaultDashboardPageType.DashboardPropertiesPage, unlockedVaultModel) { Transition = new SuppressTransitionModel() }),
                SectionName = "Properties"
            };
        }
    }
}
