using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.WinUI.Serialization;

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
        public bool VaultAutoLock
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }
    }
}
