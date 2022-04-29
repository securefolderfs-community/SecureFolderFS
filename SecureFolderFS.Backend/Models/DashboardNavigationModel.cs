using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Backend.Models
{
    public sealed class DashboardNavigationModel : IRecipient<DashboardNavigationRequestedMessage>, IRecipient<VaultLockedMessage>
    {
        private IMessenger Messenger { get; }

        private Dictionary<VaultDashboardPageType, BaseDashboardPageViewModel?> NavigationDestinations { get; }

        public DashboardNavigationModel(IMessenger messenger)
        {
            this.Messenger = messenger;
            this.NavigationDestinations = new();
        }

        private BaseDashboardPageViewModel? NavigateToPage(VaultDashboardPageType vaultDashboardPageType, VaultViewModel vaultViewModel)
        {
            BaseDashboardPageViewModel? baseDashboardPageViewModel;
            switch (vaultDashboardPageType)
            {
                case VaultDashboardPageType.MainDashboardPage:
                    NavigationDestinations.SetAndGet(vaultDashboardPageType, out baseDashboardPageViewModel, () => new VaultMainDashboardPageViewModel(Messenger, vaultViewModel));
                    break;
                case VaultDashboardPageType.DashboardPropertiesPage:
                    NavigationDestinations.SetAndGet(vaultDashboardPageType, out baseDashboardPageViewModel, () => new VaultDashboardPropertiesPageViewModel(Messenger, vaultViewModel));
                    break;
                case VaultDashboardPageType.Undefined:
                default:
                    throw new ArgumentOutOfRangeException(nameof(vaultDashboardPageType));
            }
            
            return baseDashboardPageViewModel;
        }

        private void NavigateToPage(VaultDashboardPageType vaultDashboardPageType, BaseDashboardPageViewModel baseDashboardPageViewModel)
        {
            if (!NavigationDestinations.SetAndGet(vaultDashboardPageType, out _, () => baseDashboardPageViewModel))
            {
                // Wasn't updated, do it manually..
                NavigationDestinations[vaultDashboardPageType] = baseDashboardPageViewModel;
            }
        }

        public void Receive(DashboardNavigationRequestedMessage message)
        {
            BaseDashboardPageViewModel? baseDashboardPageViewModel;
            if (message.Value == null)
            {
                baseDashboardPageViewModel = NavigateToPage(message.VaultDashboardPageType, message.VaultViewModel);
            }
            else
            {
                NavigateToPage(message.VaultDashboardPageType, message.Value);
                baseDashboardPageViewModel = message.Value;
            }

            Messenger.Send(new DashboardNavigationFinishedMessage(baseDashboardPageViewModel!) { Transition = message.Transition });
        }

        public void Receive(VaultLockedMessage message)
        {
            NavigationDestinations.Clear();
        }
    }
}
