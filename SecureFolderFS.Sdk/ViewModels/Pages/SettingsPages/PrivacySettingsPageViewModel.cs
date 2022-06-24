using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services.Settings;

namespace SecureFolderFS.Sdk.ViewModels.Pages.SettingsPages
{
    public sealed class PrivacySettingsPageViewModel : ObservableObject
    {
        private IPrivacySettingsService PrivacySettingsService { get; } = Ioc.Default.GetRequiredService<IPrivacySettingsService>();

        public bool AutoLockVaults
        {
            get => PrivacySettingsService.AutoLockVaults;
            set => PrivacySettingsService.AutoLockVaults = value;
        }

        public bool IsTelemetryEnabled
        {
            get => PrivacySettingsService.IsTelemetryEnabled;
            set => PrivacySettingsService.IsTelemetryEnabled = value;
        }
    }
}
