using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            DashboardNavigationItems.Add(new());
            DashboardNavigationItems.Add(new() { SectionName = "test2", IsLeading = true});
        }
    }
}
