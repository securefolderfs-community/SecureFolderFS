using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    public sealed class PrivacySettingsViewModel : ObservableObject
    {
        private ISettingsService SettingsService { get; } = Ioc.Default.GetRequiredService<ISettingsService>();

        public bool AutoLockVaults
        {
            get => SettingsService.UserSettings.AutoLockVaults;
            set => SettingsService.UserSettings.AutoLockVaults = value;
        }

        public bool IsTelemetryEnabled
        {
            get => SettingsService.UserSettings.IsTelemetryEnabled;
            set => SettingsService.UserSettings.IsTelemetryEnabled = value;
        }
    }
}
