using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Backend.Enums;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Backend.Messages;
using SecureFolderFS.Backend.ViewModels.Pages.DashboardPages;

namespace SecureFolderFS.Backend.Models
{
    public sealed class DashboardNavigationModel : IRecipient<DashboardNavigationRequestedMessage>, IRecipient<LockVaultRequestedMessage>
    {
        private readonly UnlockedVaultModel _unlockedVaultModel;

        private Dictionary<VaultDashboardPageType, BaseDashboardPageViewModel?> NavigationDestinations { get; }

        public DashboardNavigationModel(UnlockedVaultModel unlockedVaultModel)
        {
            this._unlockedVaultModel = unlockedVaultModel;

            NavigationDestinations = new();

            WeakReferenceMessenger.Default.Register<DashboardNavigationRequestedMessage>(this);
            WeakReferenceMessenger.Default.Register<LockVaultRequestedMessage>(this);
        }

        private BaseDashboardPageViewModel? NavigateToPage(VaultDashboardPageType vaultDashboardPageType, UnlockedVaultModel unlockedVaultModel)
        {
            BaseDashboardPageViewModel? baseDashboardPageViewModel;
            switch (vaultDashboardPageType)
            {
                case VaultDashboardPageType.MainDashboardPage:
                    NavigationDestinations.SetAndGet(vaultDashboardPageType, out baseDashboardPageViewModel, () => new VaultMainDashboardPageViewModel(unlockedVaultModel));
                    break;
                case VaultDashboardPageType.DashboardPropertiesPage:
                    NavigationDestinations.SetAndGet(vaultDashboardPageType, out baseDashboardPageViewModel, () => new VaultDashboardPropertiesPageViewModel(unlockedVaultModel));
                    break;
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
            if (message.UnlockedVaultModel != _unlockedVaultModel)
            {
                return;
            }

            BaseDashboardPageViewModel? baseDashboardPageViewModel;
            if (message.Value == null)
            {
                baseDashboardPageViewModel = NavigateToPage(message.VaultDashboardPageType, message.UnlockedVaultModel);
            }
            else
            {
                NavigateToPage(message.VaultDashboardPageType, message.Value);
                baseDashboardPageViewModel = message.Value;
            }

            WeakReferenceMessenger.Default.Send(new DashboardNavigationFinishedMessage(baseDashboardPageViewModel!) { From = message.From });
        }

        public void Receive(LockVaultRequestedMessage message)
        {
            if (_unlockedVaultModel.VaultModel == message.Value)
            {
                NavigationDestinations.Clear();
            }
        }
    }
}
