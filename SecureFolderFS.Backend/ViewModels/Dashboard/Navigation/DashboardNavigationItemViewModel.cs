using CommunityToolkit.Mvvm.ComponentModel;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels.Dashboard.Navigation
{
    public sealed class DashboardNavigationItemViewModel : ObservableObject, IDashboardNavigationItemSource
    {
        public int Index { get; set; }

        public Action<DashboardNavigationItemViewModel?> NavigationAction { get; init; }

        private string? _SectionName;
        public string? SectionName
        {
            get => _SectionName;
            set => SetProperty(ref _SectionName, value);
        }

        public bool IsLeading { get; set; }
    }
}
