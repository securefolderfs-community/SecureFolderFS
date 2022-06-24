using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Sdk.ViewModels.Settings.Banners;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages
{
    public sealed class PreferencesSettingsPageViewModel : ObservableObject
    {
        private IPreferencesSettingsService PreferencesSettingsService { get; } = Ioc.Default.GetRequiredService<IPreferencesSettingsService>();

        public FileSystemBannerViewModel BannerViewModel { get; }

        public bool StartOnSystemStartup
        {
            get => PreferencesSettingsService.StartOnSystemStartup;
            set => PreferencesSettingsService.StartOnSystemStartup = value;
        }

        public bool ContinueOnLastVault
        {
            get => PreferencesSettingsService.ContinueOnLastVault;
            set => PreferencesSettingsService.ContinueOnLastVault = value;
        }

        public bool OpenFolderOnUnlock
        {
            get => PreferencesSettingsService.OpenFolderOnUnlock;
            set => PreferencesSettingsService.OpenFolderOnUnlock = value;
        }

        public PreferencesSettingsPageViewModel()
        {
            BannerViewModel = new();
        }
    }
}
