using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Backend.Services.Settings;
using SecureFolderFS.Backend.ViewModels.Controls.FileSystemInfoBars;
using SecureFolderFS.Core.Enums;

namespace SecureFolderFS.Backend.ViewModels.Pages.SettingsDialog
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
            this.ActiveFileSystemInfoBarViewModel = new();
        }
    }
}
