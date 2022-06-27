using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="IPrivacySettingsService"/>
    internal sealed class PrivacySettingsService : SharedSettingsModel, IPrivacySettingsService
    {
        public PrivacySettingsService(ISettingsDatabaseModel originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsModel)
        {
            SettingsDatabase = originSettingsDatabase;
        }

        /// <inheritdoc/>
        public bool AutoLockVaults
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool IsTelemetryEnabled
        {
            get => GetSetting(() => false); // TODO: Enable as default
            set => SetSetting(value);
        }
    }
}
