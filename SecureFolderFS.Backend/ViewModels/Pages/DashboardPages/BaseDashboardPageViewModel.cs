using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Dashboard.Navigation;

namespace SecureFolderFS.Backend.ViewModels.Pages.DashboardPages
{
    public abstract class BaseDashboardPageViewModel : ObservableObject, IDashboardNavigationItemSource
    {
        public UnlockedVaultModel UnlockedVaultModel { get; }

        public abstract int Index { get; }

        public abstract Action<DashboardNavigationItemViewModel?> NavigationAction { get; }

        public abstract string SectionName { get; }

        public BaseDashboardPageViewModel(UnlockedVaultModel unlockedVaultModel)
        {
            this.UnlockedVaultModel = unlockedVaultModel;
        }
    }
}
