using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject
    {
        protected IMessenger Messenger { get; }

        protected VaultViewModel VaultViewModel { get; }

        protected BaseDashboardPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
        {
            Messenger = messenger;
            VaultViewModel = vaultViewModel;
        }
    }
}
