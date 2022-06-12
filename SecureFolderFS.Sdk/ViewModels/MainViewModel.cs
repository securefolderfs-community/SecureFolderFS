using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.ViewModels.Pages;
using SecureFolderFS.Sdk.ViewModels.Sidebar;

namespace SecureFolderFS.Sdk.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        public SidebarViewModel SidebarViewModel { get; }

        public SavedVaultsModel SavedVaultsModel { get; }

        private BasePageViewModel? _ActivePageViewModel;
        public BasePageViewModel? ActivePageViewModel
        {
            get => _ActivePageViewModel;
            set => SetProperty(ref _ActivePageViewModel, value);
        }

        public MainViewModel()
        {
            SidebarViewModel = new();
            SavedVaultsModel = new()
            {
                InitializableSource = SidebarViewModel
            };
        }
    }
}
