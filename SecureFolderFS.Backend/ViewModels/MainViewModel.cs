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

#nullable enable

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

        public async Task EnsureLateApplication()
        {
            if (!await CheckAvailability())
            {
                return;
            }

            AsyncExtensions.RunAndForget(() =>
            {
                SavedVaultsModel.Initialize();
            });
        }

        private async Task<bool> CheckAvailability()
        {
            var dokanyAvailability = FileSystemAvailabilityHelpers.IsDokanyAvailable();
            if (dokanyAvailability != FileSystemAvailabilityErrorType.FileSystemAvailable)
            {
                var dokanyDialogViewModel = new DokanyDialogViewModel();

                if (dokanyAvailability.HasFlag(FileSystemAvailabilityErrorType.ModuleNotAvailable)
                    || dokanyAvailability.HasFlag(FileSystemAvailabilityErrorType.DriverNotAvailable))
                {
                    dokanyDialogViewModel.ErrorText = "Dokany has not been detected. Please install Dokany to continue using SecureFolderFS.";
                }
                else if (dokanyAvailability == FileSystemAvailabilityErrorType.VersionTooLow)
                {
                    dokanyDialogViewModel.ErrorText = "The installed version of Dokany is outdated. Please update Dokany to match requested version.";
                }
                else if (dokanyAvailability == FileSystemAvailabilityErrorType.VersionTooHigh)
                {
                    dokanyDialogViewModel.ErrorText = "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany.";
                }
                else
                {
                    dokanyDialogViewModel.ErrorText = "SecureFolderFS cannot work with installed Dokany version. Please install requested version of Dokany.";
                }

                await DialogService.ShowDialog(dokanyDialogViewModel);

                return false;
            }

            return true;
        }
    }
}
