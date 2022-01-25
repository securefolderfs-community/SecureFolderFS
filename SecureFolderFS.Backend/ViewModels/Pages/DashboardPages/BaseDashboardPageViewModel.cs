using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Models;

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public abstract class BaseDashboardPageViewModel : ObservableObject
    {
        public VaultModel VaultModel { get; }

        public BaseDashboardPageViewModel(VaultModel vaultModel)
        {
            VaultModel = vaultModel;
        }
    }
}
