using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class DashboardNavigationItemViewModel : ObservableObject
    {
        private string _SectionName = "test";
        public string SectionName
        {
            get => _SectionName;
            set => SetProperty(ref _SectionName, value);
        }

        public bool IsLeading { get; set; }

        public DashboardNavigationItemViewModel()
        {

        }
    }
}
