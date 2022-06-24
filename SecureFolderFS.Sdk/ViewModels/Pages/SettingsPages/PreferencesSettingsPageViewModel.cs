using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.ViewModels.Controls.FileSystemInfoBars;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages
{
    public sealed class PreferencesSettingsPageViewModel : ObservableObject
    {
        private IPreferencesSettingsService PreferencesSettingsService { get; } = Ioc.Default.GetRequiredService<IPreferencesSettingsService>();

        public ActiveFileSystemInfoBarViewModel ActiveFileSystemInfoBarViewModel { get; }

        public FileSystemAdapterType ActiveFileSystemAdapter
        {
            get => PreferencesSettingsService.ActiveFileSystemAdapter;
            set
            {
                if (PreferencesSettingsService.ActiveFileSystemAdapter != value)
                {
                    PreferencesSettingsService.ActiveFileSystemAdapter = value;

                    ActiveFileSystemInfoBarViewModel.ConfigureFileSystem(value);
                }
            }
        }

        public bool StartOnSystemStartup
        {
            get => PreferencesSettingsService.StartOnSystemStartup;
            set
            {
                if (PreferencesSettingsService.StartOnSystemStartup != value)
                    PreferencesSettingsService.StartOnSystemStartup = value;
            }
        }

        public PreferencesSettingsPageViewModel()
        {
            ActiveFileSystemInfoBarViewModel = new();
        }
    }
}
