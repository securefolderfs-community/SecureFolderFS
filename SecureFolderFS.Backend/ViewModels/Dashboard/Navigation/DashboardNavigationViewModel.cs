using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class DashboardNavigationViewModel : ObservableObject
    {
        public ObservableCollection<DashboardNavigationItemViewModel> DashboardNavigationItems { get; }

        public DashboardNavigationViewModel()
        {
            DashboardNavigationItems = new();
        }

        public void SetNavigation(DashboardNavigationItemViewModel dashboardNavigationItemViewModel)
        {
            ArgumentNullException.ThrowIfNull(dashboardNavigationItemViewModel);

            DashboardNavigationItems.Clear();

            AppendNavigation(dashboardNavigationItemViewModel);
        }

        public void AppendNavigation(DashboardNavigationItemViewModel dashboardNavigationItemViewModel)
        {
            ArgumentNullException.ThrowIfNull(dashboardNavigationItemViewModel);

            var current = dashboardNavigationItemViewModel;
            while (current != null)
            {
                current.IsLeading = current.Parent == null;
                DashboardNavigationItems.Add(current);

                current = current.Parent;
            }
        }
    }
}
