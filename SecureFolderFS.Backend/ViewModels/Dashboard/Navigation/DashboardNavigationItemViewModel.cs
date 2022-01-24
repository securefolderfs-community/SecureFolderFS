using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class DashboardNavigationItemViewModel : ObservableObject
    {
        public DashboardNavigationItemViewModel? Parent { get; init; }

        public Action? NavigationAction { get; init; }

        private string? _SectionName;
        public string? SectionName
        {
            get => _SectionName;
            set => SetProperty(ref _SectionName, value);
        }

        public bool IsLeading { get; set; }
    }
}
