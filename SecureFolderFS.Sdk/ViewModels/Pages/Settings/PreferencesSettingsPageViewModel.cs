using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.ViewModels.Settings.Banners;

namespace SecureFolderFS.Sdk.ViewModels.Pages.Settings
{
    public sealed class PreferencesSettingsPageViewModel : ObservableObject
    {
        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public FileSystemBannerViewModel BannerViewModel { get; }

        public bool StartOnSystemStartup
        {
            get => SettingsService.UserSettings.StartOnSystemStartup;
            set => SettingsService.UserSettings.StartOnSystemStartup = value;
        }

        public bool ContinueOnLastVault
        {
            get => SettingsService.UserSettings.ContinueOnLastVault;
            set => SettingsService.UserSettings.ContinueOnLastVault = value;
        }

        public bool OpenFolderOnUnlock
        {
            get => SettingsService.UserSettings.OpenFolderOnUnlock;
            set => SettingsService.UserSettings.OpenFolderOnUnlock = value;
        }

        public PreferencesSettingsPageViewModel()
        {
            BannerViewModel = new();
        }
    }
}
