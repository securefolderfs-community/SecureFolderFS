using CommunityToolkit.Mvvm.ComponentModel;

namespace SecureFolderFS.UI.UserControls.BreadcrumbBar
{
    public sealed partial class OrderedBreadcrumbBarItem : ObservableObject
    {
        [ObservableProperty] private string _Name;
        [ObservableProperty] private bool _IsLeading;

        public OrderedBreadcrumbBarItem(string name, bool isLeading)
        {
            Name = name;
            IsLeading = isLeading;
        }
    }
}
