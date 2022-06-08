using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.ViewModels.Controls.FileSystemInfoBars;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsDialog
{
    public sealed class PreferencesSettingsPageViewModel : BaseSettingsDialogPageViewModel
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

        public PreferencesSettingsPageViewModel()
        {
            ActiveFileSystemInfoBarViewModel = new();
        }
    }
}
