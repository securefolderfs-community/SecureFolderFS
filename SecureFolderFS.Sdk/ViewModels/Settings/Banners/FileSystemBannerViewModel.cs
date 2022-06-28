using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Core.Enums;
using SecureFolderFS.Core.Helpers;
using SecureFolderFS.Sdk.Enums;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.Sdk.ViewModels.Controls;
using SecureFolderFS.Sdk.ViewModels.Settings.InfoBars;

namespace SecureFolderFS.Sdk.ViewModels.Settings.Banners
{
    public sealed class FileSystemBannerViewModel : ObservableObject
    {
        private IPreferencesSettingsService PreferencesSettingsService { get; } = Ioc.Default.GetRequiredService<IPreferencesSettingsService>();

        private InfoBarViewModel? _InfoBarViewModel;
        public InfoBarViewModel? InfoBarViewModel
        {
            get => _InfoBarViewModel;
            set => SetProperty(ref _InfoBarViewModel, value);
        }

        public FileSystemAdapterType PreferredFileSystemAdapter
        {
            get => PreferencesSettingsService.PreferredFileSystemAdapter;
            set => PreferencesSettingsService.PreferredFileSystemAdapter = value;
        }

        public void UpdateAdapterStatus()
        {
            switch (PreferredFileSystemAdapter)
            {
                case FileSystemAdapterType.DokanAdapter:
                {
                    var result = FileSystemAvailabilityHelpers.IsDokanyAvailable();
                    if (result != FileSystemAvailabilityErrorType.FileSystemAvailable)
                    {
                        InfoBarViewModel = new DokanyInfoBarViewModel();
                        InfoBarViewModel.IsOpen = true;
                        InfoBarViewModel.InfoBarSeverity = InfoBarSeverityType.Error;
                        InfoBarViewModel.CanBeClosed = false;

                        const string DOKANY_NOT_FOUND = "Dokany has not been detected. Please install Dokany to continue using SecureFolderFS.";
                        InfoBarViewModel.Message = result switch
                        {
                            FileSystemAvailabilityErrorType.ModuleNotAvailable => DOKANY_NOT_FOUND,
                            FileSystemAvailabilityErrorType.DriverNotAvailable => DOKANY_NOT_FOUND,
                            FileSystemAvailabilityErrorType.VersionTooLow => "The installed version of Dokany is outdated. Please update Dokany to match requested version.",
                            FileSystemAvailabilityErrorType.VersionTooHigh => "The installed version of Dokany is not compatible with SecureFolderFS version. Please install requested version of Dokany.",
                            _ => "SecureFolderFS cannot work with installed Dokany version. Please install requested version of Dokany."
                        };
                    }
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(PreferredFileSystemAdapter));
            }
        }
    }
}
