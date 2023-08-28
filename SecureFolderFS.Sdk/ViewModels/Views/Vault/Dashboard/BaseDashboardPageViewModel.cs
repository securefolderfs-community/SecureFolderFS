using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Vault;

namespace SecureFolderFS.Sdk.ViewModels.Views.Vault.Dashboard
{
    public abstract class BaseDashboardPageViewModel : BasePageViewModel
    {
        protected UnlockedVaultViewModel UnlockedVaultViewModel { get; }

        protected INavigationService DashboardNavigationService { get; }

        protected BaseDashboardPageViewModel(UnlockedVaultViewModel unlockedVaultViewModel, INavigationService dashboardNavigationService)
        {
            UnlockedVaultViewModel = unlockedVaultViewModel;
            DashboardNavigationService = dashboardNavigationService;
        }
    }
}
