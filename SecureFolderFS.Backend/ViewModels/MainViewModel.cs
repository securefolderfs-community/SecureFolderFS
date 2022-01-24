using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Sidebar;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        public SidebarViewModel SidebarViewModel { get; }

        public NavigationModel NavigationModel { get; }

        public MainViewModel()
        {
            SidebarViewModel = new();
            NavigationModel = new();
        }
    }
}
