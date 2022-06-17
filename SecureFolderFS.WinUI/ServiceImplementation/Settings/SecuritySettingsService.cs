using SecureFolderFS.Sdk.Models;
using SecureFolderFS.Sdk.Services.Settings;
using SecureFolderFS.WinUI.Serialization;

namespace SecureFolderFS.WinUI.ServiceImplementation.Settings
{
    /// <inheritdoc cref="ISecuritySettingsService"/>
    internal sealed class SecuritySettingsService : SharedSettingsModel, ISecuritySettingsService
    {
        public SecuritySettingsService(ISettingsDatabaseModel originSettingsDatabase, ISettingsModel originSettingsModel)
            : base(originSettingsModel)
        {
            SettingsDatabase = originSettingsDatabase;
        }

        /// <inheritdoc/>
        public bool EnableAuthentication
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }

        /// <inheritdoc/>
        public bool AutomaticallyLockVaults
        {
            get => GetSetting(() => false);
            set => SetSetting(value);
        }
    }
}
