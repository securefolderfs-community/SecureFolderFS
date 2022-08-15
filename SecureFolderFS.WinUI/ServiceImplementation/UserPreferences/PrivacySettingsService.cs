using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;
using SecureFolderFS.WinUI.AppModels;

namespace SecureFolderFS.WinUI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IPrivacySettingsService"/>
    internal sealed class PrivacySettingsService : SharedSettingsModel, IPrivacySettingsService
    {
        public PrivacySettingsService(IDatabaseModel<string> originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsDatabase, originSettingsModel)
        {
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
