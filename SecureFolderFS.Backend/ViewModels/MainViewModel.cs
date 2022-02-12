using CommunityToolkit.Mvvm.ComponentModel;
using SecureFolderFS.Backend.Extensions;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.ViewModels.Pages;
using SecureFolderFS.Backend.ViewModels.Sidebar;

#nullable enable

namespace SecureFolderFS.Backend.ViewModels
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

        public void EnsureLateApplication()
        {
            AsyncExtensions.RunAndForget(() =>
            {
                SavedVaultsModel.Initialize();
            });
        }
    }
}
