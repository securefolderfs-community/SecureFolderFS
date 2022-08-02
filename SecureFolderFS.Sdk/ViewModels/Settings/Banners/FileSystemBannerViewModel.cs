using System.Collections.Generic;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services.UserPreferences;

namespace SecureFolderFS.Sdk.ViewModels.Settings.Banners
{
    public sealed class FileSystemBannerViewModel : ObservableObject
    {
        private IPreferencesSettingsService PreferencesSettingsService { get; } = Ioc.Default.GetRequiredService<IPreferencesSettingsService>();

        public ObservableCollection<FileSystemAdapterItemViewModel> FileSystemAdapters { get; }

        public string PreferredFileSystemId
        {
            get => PreferencesSettingsService.PreferredFileSystemId;
            set => PreferencesSettingsService.PreferredFileSystemId = value;
        }

        public FileSystemBannerViewModel()
        {
            FileSystemAdapters = new();
        }
    }
}
