using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Shared.Extensions;
using SecureFolderFS.Backend.Models;
using SecureFolderFS.Backend.Services;
using SecureFolderFS.Backend.ViewModels.Dialogs;
using SecureFolderFS.Backend.ViewModels.Pages;
using SecureFolderFS.Backend.ViewModels.Sidebar;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Helpers;

namespace SecureFolderFS.Backend.ViewModels
{
    public sealed class MainViewModel : ObservableObject
    {
        private IDialogService DialogService { get; } = Ioc.Default.GetRequiredService<IDialogService>();

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
