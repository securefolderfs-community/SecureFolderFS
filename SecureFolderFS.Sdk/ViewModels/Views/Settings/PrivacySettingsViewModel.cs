using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using System.ComponentModel;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ITelemetryService>, Inject<ISettingsService>]
    public sealed partial class PrivacySettingsViewModel : BasePageViewModel
    {
        private IUserSettings UserSettings => SettingsService.UserSettings;

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
            ServiceProvider = Ioc.Default;
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
