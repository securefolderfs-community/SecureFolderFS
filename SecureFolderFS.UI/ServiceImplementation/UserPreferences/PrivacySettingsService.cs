using SecureFolderFS.Sdk.AppModels;
using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.UserPreferences;

namespace SecureFolderFS.UI.ServiceImplementation.UserPreferences
{
    /// <inheritdoc cref="IPrivacySettingsService"/>
    public sealed class PrivacySettingsService : SharedSettingsModel, IPrivacySettingsService
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
