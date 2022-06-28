using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using SecureFolderFS.Sdk.ViewModels.Dashboard.Navigation;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, IDisposable
    {
        protected IMessenger Messenger { get; }

        protected VaultViewModel VaultViewModel { get; }

        public NavigationItemViewModel? NavigationItemViewModel { get; protected init; }

        protected BaseDashboardPageViewModel(IMessenger messenger, VaultViewModel vaultViewModel)
        {
            Messenger = messenger;
            VaultViewModel = vaultViewModel;
        }

        public virtual void Dispose()
        {
            VaultViewModel.VaultInstance?.Dispose();
            VaultViewModel.VaultInstance = null;
        }
    }
}
