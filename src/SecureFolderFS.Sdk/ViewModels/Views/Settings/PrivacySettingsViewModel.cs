using CommunityToolkit.Mvvm.DependencyInjection;
using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ITelemetryService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class PrivacySettingsViewModel : BaseSettingsViewModel
    {
        public PrivacySettingsViewModel()
        {
            ServiceProvider = Ioc.Default;
            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

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

        /// <inheritdoc/>
        public override Task InitAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
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
