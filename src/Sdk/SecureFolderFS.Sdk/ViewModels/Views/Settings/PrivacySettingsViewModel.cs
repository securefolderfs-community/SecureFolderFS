using SecureFolderFS.Sdk.Attributes;
using SecureFolderFS.Sdk.Services;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.Shared;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SecureFolderFS.Sdk.Enums;

namespace SecureFolderFS.Sdk.ViewModels.Views.Settings
{
    [Inject<ITelemetryService>, Inject<ISettingsService>]
    [Bindable(true)]
    public sealed partial class PrivacySettingsViewModel : BaseSettingsViewModel
    {
        public PrivacySettingsViewModel()
        {
            ServiceProvider = DI.Default;
            UserSettings.PropertyChanged += UserSettings_PropertyChanged;
        }

        public bool DisableRecentAccess
        {
            get => UserSettings.DisableRecentAccess;
            set => UserSettings.DisableRecentAccess = value;
        }

        public bool LockOnSystemLock
        {
            get => UserSettings.LockOnSystemLock;
            set => UserSettings.LockOnSystemLock = value;
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
            if (e.PropertyName != nameof(IUserSettings.IsTelemetryEnabled))
                return;

            if (IsTelemetryEnabled)
            {
                await TelemetryService.EnableTelemetryAsync();
                TelemetryService.TrackMessage("[Telemetry] Enabled", Severity.Default);
            }
            else
            {
                TelemetryService.TrackMessage("[Telemetry] Disabled", Severity.Default);
                await TelemetryService.DisableTelemetryAsync();
            }
        }
    }
}
