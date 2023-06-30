using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.SettingsPersistence;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    public sealed class PrivacySettingsViewModel : BasePageViewModel
    {
        private IUserSettings UserSettings { get; } = Ioc.Default.GetRequiredService<ISettingsService>().UserSettings;

        private ITelemetryService TelemetryService { get; } = Ioc.Default.GetRequiredService<ITelemetryService>();

        public bool AutoLockVaults
        {
            get => UserSettings.AutoLockVaults;
            set => UserSettings.AutoLockVaults = value;
        }

        public bool IsTelemetryEnabled
        {
            get => UserSettings.IsTelemetryEnabled;
            set => UserSettings.IsTelemetryEnabled = value;
        }

        public PrivacySettingsViewModel()
        {
            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        private async void UserSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IUserSettings.IsTelemetryEnabled) && IsTelemetryEnabled)
            {
                await TelemetryService.EnableTelemetryAsync();
                TelemetryService.TrackEvent("Telemetry manually enabled");
            }
            else
            {
                TelemetryService.TrackEvent("Telemetry manually disabled");
                await TelemetryService.DisableTelemetryAsync();
            }
        }
    }
}
