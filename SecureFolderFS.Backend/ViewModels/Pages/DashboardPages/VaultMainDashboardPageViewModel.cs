using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Dashboard;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public sealed class VaultMainDashboardPageViewModel : BaseDashboardPageViewModel
    {
        public VaultHealthViewModel VaultHealthViewModel { get; }

        public VaultMainDashboardPageViewModel(VaultModel vaultModel)
            : base(vaultModel)
        {
            VaultHealthViewModel = new();
        }
    }
}
